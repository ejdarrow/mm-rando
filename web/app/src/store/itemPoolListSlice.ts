import { createItemListSlice, ItemListStore } from "./createItemListSlice"
import { RootState } from "./store"

const itemPoolListSlice = createItemListSlice('itemPool')

export const itemPoolListStore: ItemListStore = {
  selector: (state: RootState) => state.itemPool.value,
  slice: itemPoolListSlice
}

export default itemPoolListSlice
