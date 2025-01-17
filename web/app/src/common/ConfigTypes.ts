import { Matrix } from './Matrix';
import { ItemPoolJson, ItemPoolColumnJson, ItemPoolRowJson } from './JsonTypes';
import { ItemListBitMask } from './ItemList';
import { CategoryGroupContainer, CategoryGroupContainerBuilder } from './ItemCategory';

export class ItemMatrixVector<T> {
  data: Readonly<T>;
  count: number;
  index: number;
  bitMask: ItemListBitMask;
  orphaned: ItemRepr[];

  constructor(data: T, count: number, index: number) {
    this.data = data;
    this.count = count;
    this.index = index;
    this.bitMask = ItemListBitMask.empty();
    this.orphaned = [];
  }
}

export class ItemListColumnRepr extends ItemMatrixVector<ItemPoolColumnJson> {
  getKey() {
    return 'location_type:' + this.data.Category;
  }
}

export class ItemListRowRepr extends ItemMatrixVector<ItemPoolRowJson> {
  getKey() {
    return 'item_type:' + this.data.Category;
  }
}

export class ItemRepr {
  index: number;
  columnIndex: number;
  rowIndex: number;
  itemName: string;
  locationName: string;

  constructor(index: number, columnIndex: number, rowIndex: number, itemName: string, locationName: string) {
    this.index = index;
    this.columnIndex = columnIndex;
    this.rowIndex = rowIndex;
    this.itemName = itemName;
    this.locationName = locationName;
  }

  /// Compare two items by location name.
  static compare(a: ItemRepr, b: ItemRepr) {
    return a.locationName.localeCompare(b.locationName);
  }
}

class ItemMatrixCell {
  readonly items: ItemRepr[];
  bitMask: ItemListBitMask;

  constructor(items?: ItemRepr[], bitMask?: ItemListBitMask) {
    this.items = items ?? [];
    this.bitMask = bitMask ?? ItemListBitMask.empty();
  }

  updateBitMask() {
    const bitIndexes = this.items.map((x) => x.index);
    this.bitMask = ItemListBitMask.fromBits(bitIndexes);
  }
}

class ItemMatrix extends Matrix<ItemMatrixCell> {
  /// Create an ItemListBitMask for a given grid vector (column or row).
  createVectorBitMask<T>(vector: ItemMatrixVector<T>, generator: Generator<ItemMatrixCell>) {
    const array = Array<ItemListBitMask>();
    for (let cell of generator) {
      // Push matrix cell bit mask.
      array.push(cell?.bitMask);
    }
    if (vector.orphaned.length > 0) {
      // Push orphaned list bit mask.
      const orphanedBitMask = ItemListBitMask.fromBits(vector.orphaned.map((x) => x.index));
      array.push(orphanedBitMask);
    }
    return ItemListBitMask.bitwiseOrAll(array.filter((x) => x !== undefined));
  }

  createColumnBitMask(column: ItemListColumnRepr) {
    return this.createVectorBitMask(column, this.getColumnGenerator(column.index));
  }

  createRowBitMask(row: ItemListRowRepr) {
    return this.createVectorBitMask(row, this.getRowGenerator(row.index));
  }

  push(value: ItemRepr, colIndex: number, rowIndex: number) {
    const index = this.calcFlatIndex(colIndex, rowIndex);
    if (this.data[index] === undefined) {
      this.data[index] = new ItemMatrixCell();
    }
    return this.data[index].items.push(value);
  }

  updateBitMasks(columnReprs: ItemListColumnRepr[], rowReprs: ItemListRowRepr[]) {
    this.updateCellBitMasks();
    for (let i = 0; i < this.colCount; i++) {
      columnReprs[i].bitMask = this.createColumnBitMask(columnReprs[i]);
    }
    for (let i = 0; i < this.rowCount; i++) {
      rowReprs[i].bitMask = this.createRowBitMask(rowReprs[i]);
    }
  }

  updateCellBitMasks() {
    // Linear loop to update all bit masks.
    for (let i = 0; i < this.data.length; i++) {
      if (this.data[i] !== undefined) {
        this.data[i].updateBitMask();
      }
    }
  }
}

export class ItemListRepr {
  columns: ItemListColumnRepr[];
  rows: ItemListRowRepr[];

  /// Item flat array representation indexed by item integer value.
  items: ItemRepr[];

  /// Item grid representation using location category and item category as axies.
  matrix: ItemMatrix;

  /// Container for category groups which map to item groups.
  categoryGroups: CategoryGroupContainer;

  constructor(
    columns: ItemListColumnRepr[],
    rows: ItemListRowRepr[],
    items: ItemRepr[],
    matrix: ItemMatrix,
    categoryGroups: CategoryGroupContainer
  ) {
    this.columns = columns;
    this.rows = rows;
    this.items = items;
    this.matrix = matrix;
    this.categoryGroups = categoryGroups;
  }

  hasCell(colIndex: number, rowIndex: number) {
    const cell = this.matrix.get(colIndex, rowIndex)
    return cell !== undefined && cell.items.length > 0
  }

  static fromJson(data: ItemPoolJson) {
    const columns = data.Columns.map((col, idx) => {
      return new ItemListColumnRepr(col, 0, idx);
    });
    const rows = data.Rows.map((row, idx) => {
      return new ItemListRowRepr(row, 0, idx);
    });

    // Flat item array representation.
    const items = Array<ItemRepr>(data.Items.length);

    // Matrix representation.
    const matrix = new ItemMatrix(columns.length, rows.length);

    const categoryGroupBuilder = new CategoryGroupContainerBuilder();

    data.Items.forEach((x) => {
      const colIdx = columns.findIndex((location) => {
        return x.LocationCategory === location.data.Category;
      });
      const rowIdx = rows.findIndex((item) => {
        return x.ItemCategory === item.data.Category;
      });

      const column = colIdx >= 0 ? columns[colIdx] : null;
      const row = rowIdx >= 0 ? rows[rowIdx] : null;

      const item = new ItemRepr(x.Index, colIdx, rowIdx, x.ItemName, x.LocationName);

      // Ensure that item index is in bounds and non-duplicate.
      if (item.index < 0 || item.index >= items.length) {
        throw Error(`Item index is out-of-bounds: [${item.index}]`);
      } else if (items[item.index] !== undefined) {
        throw Error(`Duplicate item index: [${item.index}]`);
      }

      // Push into array at appropriate matrix cell, or orphaned array if no cell.
      if (item.columnIndex >= 0 && item.rowIndex >= 0) {
        matrix.push(item, item.columnIndex, item.rowIndex);
      } else if (item.columnIndex >= 0) {
        columns[item.columnIndex].orphaned.push(item);
      } else if (item.rowIndex >= 0) {
        rows[item.rowIndex].orphaned.push(item);
      } else {
        throw Error(`Item does not belong to a cell, column or row: [${item.index}]`);
      }

      // Increment column and/or row counter.
      if (column !== null) {
        column.count++;
      }
      if (row !== null) {
        row.count++;
      }

      // Append item to category groups.
      categoryGroupBuilder.appendByLocationType(column, item);
      categoryGroupBuilder.appendByItemType(row, item);

      items[item.index] = item;
    });

    // Validate that items flat array contains no `undefined` elements.
    const indexOfUndefined = items.findIndex((x) => x === undefined);
    if (indexOfUndefined !== -1) {
      throw Error(`Item index is missing: [${indexOfUndefined}]`);
    }

    // Update all bit masks for each matrix cell, column and row.
    matrix.updateBitMasks(columns, rows);

    // Complete into CategoryGroupContainer.
    const categoryGroupContainer = categoryGroupBuilder.complete();

    return new ItemListRepr(columns, rows, items, matrix, categoryGroupContainer);
  }
}
