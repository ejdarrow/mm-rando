import { PayloadAction, createSlice } from '@reduxjs/toolkit'
import { RootState } from './store'
import {
  ItemListBitMaskObject,
  ItemListBits,
  ItemListBitsObject,
  asItemListBits,
  getEmptyItemList,
  performAllClear,
  performAllSet,
  performBitClear,
  performBitSet,
  performFromString,
  performMaskClear,
  performMaskSet,
  performWithLength,
} from '../common/ItemList'

// const transform = <T,>(state: ItemListObject, callback: (list: ItemListBits) => T) => {
//   const x = ItemListBits.fromObject(state)
//   return callback(x)
// }

export const createItemListSlice = (id: string) => createSlice({
  name: `${id}-itemList`,
  initialState: {
    value: getEmptyItemList()
  },
  reducers: {
    allClear: state => {
      state.value = performAllClear(state.value)
    },
    allSet: state => {
      state.value = performAllSet(state.value)
    },
    bitClear: (state, action: PayloadAction<number>) => {
      state.value = performBitClear(state.value, action.payload)
    },
    bitSet: (state, action: PayloadAction<number>) => {
      state.value = performBitSet(state.value, action.payload)
    },
    maskClear: (state, action: PayloadAction<ItemListBitMaskObject>) => {
      state.value = performMaskClear(state.value, action.payload)
    },
    maskSet: (state, action: PayloadAction<ItemListBitMaskObject>) => {
      state.value = performMaskSet(state.value, action.payload)
    },
    fromString: (state, action: PayloadAction<{value: string; length: number;}>) => {
      state.value = performFromString(action.payload.value, action.payload.length)
    },
    stateClear: state => {
      state.value = getEmptyItemList()
    },
    withLength: (state, action: PayloadAction<number>) => {
      state.value = performWithLength(action.payload)
    }
  }
})

/** Convenience function for wrapping into `ItemListBits`. */
export const asItemList = (obj: ItemListBitsObject): ItemListBits => asItemListBits(obj)

export type ItemListSlice = ReturnType<typeof createItemListSlice>

/// Describes how to select an ItemListBits from the store's state, and references the corresponding Slice.
/// Needed if we are to have multiple ItemListBits instances using the same reducer (item pool, junk locations).
export interface ItemListStore {
  selector: (state: RootState) => ItemListBitsObject
  slice: ItemListSlice
}
