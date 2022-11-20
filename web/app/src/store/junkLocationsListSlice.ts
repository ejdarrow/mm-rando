import { createItemListSlice, ItemListStore } from "./createItemListSlice"
import { RootState } from "./store"

const junkLocationsListSlice = createItemListSlice('junkLocations')

export const junkLocationsListStore: ItemListStore = {
  selector: (state: RootState) => state.junkLocations.value,
  slice: junkLocationsListSlice
}

export default junkLocationsListSlice
