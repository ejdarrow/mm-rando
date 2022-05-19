import { ItemPoolColumnRepr, ItemPoolItemRepr, ItemPoolRowRepr } from './ConfigTypes';
import { ExMap } from './ExMap';
import { ItemListBitMask } from './ItemList';
import { CompareFn, coalesceUndef, tuple } from './Utility';

export enum CategoryType {
  Location,
  Item,
}

export class CategoryGroupBuilder {
  categoryGroups: Map<string, ItemPoolItemRepr[]>;
  orphaned?: ItemPoolItemRepr[];

  constructor(categoryGroups?: Map<string, ItemPoolItemRepr[]>, orphaned?: ItemPoolItemRepr[]) {
    this.categoryGroups = categoryGroups ?? new Map<string, ItemPoolItemRepr[]>();
    this.orphaned = orphaned;
  }

  /// Complete into a CategoryGroup.
  complete(compareFn?: CompareFn<string>) {
    let mapping = new ExMap<string, ItemGroup>();
    this.categoryGroups.forEach((itemArray, categoryValue) => {
      const itemGroup = ItemGroup.fromItems(itemArray);
      mapping.set(categoryValue, itemGroup);
    });
    const orphaned = coalesceUndef(this.orphaned, ItemGroup.fromItems);
    return CategoryGroup.fromMapping(mapping, orphaned, compareFn);
  }

  groups() {
    return this.categoryGroups;
  }
}

export class CategoryGroupContainerBuilder {
  categoryGroups: Map<CategoryType, CategoryGroupBuilder>;

  constructor(categoryGroups?: Map<CategoryType, CategoryGroupBuilder>) {
    this.categoryGroups = categoryGroups ?? new Map<CategoryType, CategoryGroupBuilder>();
  }

  /// Get a CategoryGroupBuilder, creating it if it does not exist.
  createCategoryGroupBuilder(categoryType: CategoryType) {
    let categoryValueMap = this.categoryGroups.get(categoryType);

    if (categoryValueMap === undefined) {
      categoryValueMap = new CategoryGroupBuilder();
      this.categoryGroups.set(categoryType, categoryValueMap);
    }

    return categoryValueMap;
  }

  /// Append an item to a CategoryGroupBuilder.
  append(categoryType: CategoryType, categoryValue: string, value: ItemPoolItemRepr) {
    let categoryValueMap = this.createCategoryGroupBuilder(categoryType);

    if (categoryValueMap?.groups().get(categoryValue) === undefined) {
      categoryValueMap?.groups().set(categoryValue, [value]);
    } else {
      categoryValueMap?.groups().get(categoryValue)?.push(value);
    }
  }

  /// Append an orphaned item to a CategoryGroupBuilder.
  appendOrphan(categoryType: CategoryType, value: ItemPoolItemRepr) {
    let categoryGroupBuilder = this.createCategoryGroupBuilder(categoryType);
    if (categoryGroupBuilder.orphaned === undefined) {
      categoryGroupBuilder.orphaned = [value];
    } else {
      categoryGroupBuilder.orphaned.push(value);
    }
  }

  /// Append an item based on Location type.
  appendByLocationType(column: ItemPoolColumnRepr | null, item: ItemPoolItemRepr) {
    const categoryType = CategoryType.Location;
    if (column !== null) {
      this.append(categoryType, column.data.Name, item);
    } else {
      this.appendOrphan(categoryType, item);
    }
  }

  /// Append an item based on Item type.
  appendByItemType(row: ItemPoolRowRepr | null, item: ItemPoolItemRepr) {
    const categoryType = CategoryType.Item;
    if (row !== null) {
      this.append(categoryType, row.data.Name, item);
    } else {
      this.appendOrphan(categoryType, item);
    }
  }

  /// Complete into a CategoryGroupContainer.
  complete() {
    let result = new ExMap<CategoryType, CategoryGroup>();
    for (const [categoryType, categoryValueMapping] of this.categoryGroups) {
      const categoryGroup = categoryValueMapping.complete();
      result.set(categoryType, categoryGroup);
    }
    return new CategoryGroupContainer(result);
  }
}

/// Mapping of category types to category groups.
export class CategoryGroupContainer {
  categoryGroups: ExMap<CategoryType, CategoryGroup>;

  constructor(categoryGroups: ExMap<CategoryType, CategoryGroup>) {
    this.categoryGroups = categoryGroups;
  }

  /// Get the CategoryGroup which is mapped to a given CategoryType.
  byCategory(categoryType: CategoryType) {
    return this.categoryGroups.getOrError(categoryType);
  }
}

/// Mapping of sub-categories to item groups.
export class CategoryGroup {
  mapping: ExMap<string, ItemGroup>;
  orphaned?: ItemGroup;
  keysDefault: string[];
  keysSorted: string[];
  compareFn?: CompareFn<string>;

  constructor(
    mapping: ExMap<string, ItemGroup>,
    keysDefault: string[],
    keysSorted: string[],
    orphaned?: ItemGroup,
    compareFn?: CompareFn<string>
  ) {
    this.mapping = mapping;
    this.keysDefault = keysDefault;
    this.keysSorted = keysSorted;
    this.orphaned = orphaned;
    this.compareFn = compareFn;
  }

  /// Return a Generator using an ordered list of keys.
  *childrenOrdered(keys: string[]): Generator<[string | null, ItemGroup], any, undefined> {
    for (const key of keys) {
      yield tuple(key, this.mapping.getOrError(key));
    }
    if (this.orphaned) {
      yield tuple(null, this.orphaned);
    }
  }

  childrenDefault(): Generator<[string | null, ItemGroup], any, undefined> {
    return this.childrenOrdered(this.keysDefault);
  }

  childrenSorted(): Generator<[string | null, ItemGroup], any, undefined> {
    return this.childrenOrdered(this.keysSorted);
  }

  /// Perform a query and return the resulting subset.
  query(queryTerms: string[]) {
    const newMapping: [string, ItemGroup][] = [];
    for (const [categoryValue, itemGroup] of this.mapping) {
      const filteredItemGroup = itemGroup.query(queryTerms);
      if (filteredItemGroup.items.length !== 0) newMapping.push(tuple(categoryValue, filteredItemGroup));
    }
    const filteredMapping = new ExMap<string, ItemGroup>(newMapping);

    // Filter orphan group.
    let filteredOrphans = coalesceUndef(this.orphaned, (orphaned) => orphaned.query(queryTerms));
    if (filteredOrphans !== undefined && filteredOrphans.items.length === 0) {
      filteredOrphans = undefined;
    }

    return CategoryGroup.fromMapping(filteredMapping, filteredOrphans, this.compareFn);
  }

  static fromMapping(mapping: ExMap<string, ItemGroup>, orphaned?: ItemGroup, compareFn?: CompareFn<string>) {
    const keysDefault = Array.from(mapping.keys());
    const keysSorted = Array.from(mapping.keys()).sort(compareFn);
    return new CategoryGroup(mapping, keysDefault, keysSorted, orphaned, compareFn);
  }
}

/// Collection of items with a corresponding bit mask.
export class ItemGroup {
  items: ItemPoolItemRepr[];
  bitMask: ItemListBitMask;

  constructor(items: ItemPoolItemRepr[], bitMask: ItemListBitMask) {
    this.items = items.sort(ItemPoolItemRepr.compare);
    this.bitMask = bitMask;
  }

  /// Perform a query and return the resulting subset.
  query(queryTerms: string[]) {
    const filteredItems = this.items.filter((x) => {
      for (const queryTerm of queryTerms) {
        const term = queryTerm.toLowerCase();
        // Only queries using location name for the time being.
        if (x.locationName.toLowerCase().indexOf(term) !== -1) {
          return true;
        }
      }
      return false;
    });
    return ItemGroup.fromItems(filteredItems);
  }

  static fromItems(items: ItemPoolItemRepr[]) {
    const bitMask = ItemListBitMask.fromBits(items.map((x) => x.index));
    return new ItemGroup(items, bitMask);
  }
}
