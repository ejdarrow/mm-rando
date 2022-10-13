export class ItemPoolRowJson {
  Name: string;
  Description: string;
  Category: string;

  constructor(name: string, description: string, category: string) {
    this.Name = name;
    this.Description = description;
    this.Category = category;
  }
}

export class ItemPoolColumnJson {
  Name: string;
  Description: string;
  Category: string;

  constructor(name: string, description: string, category: string) {
    this.Name = name;
    this.Description = description;
    this.Category = category;
  }
}

export class ItemPoolItemJson {
  Index: number;
  ItemCategory: string;
  ItemName: string;
  LocationCategory: string;
  LocationName: string;

  constructor(index: number, itemCategory: string, itemName: string, locationCategory: string, locationName: string) {
    this.Index = index;
    this.ItemCategory = itemCategory;
    this.ItemName = itemName;
    this.LocationCategory = locationCategory;
    this.LocationName = locationName;
  }
}

export class ItemPoolJson {
  Columns: ItemPoolColumnJson[];
  Rows: ItemPoolRowJson[];
  Items: ItemPoolItemJson[];

  constructor(columns: ItemPoolColumnJson[], rows: ItemPoolRowJson[], items: ItemPoolItemJson[]) {
    this.Columns = columns;
    this.Rows = rows;
    this.Items = items;
  }
}

export class UserInterfaceJson {
  ItemPool: ItemPoolJson;

  constructor(itemPool: ItemPoolJson) {
    this.ItemPool = itemPool;
  }
}
