import { PayloadAction, createSlice } from '@reduxjs/toolkit'
import { ItemListBitMask, ItemListBits } from '../common/ItemList'
import { RootState } from '../store'

// const transform = <T,>(state: ItemListObject, callback: (list: ItemListBits) => T) => {
//   const x = ItemListBits.fromObject(state)
//   return callback(x)
// }

export const createItemListSlice = (id: string) => createSlice({
  name: `${id}-itemList`,
  initialState: {
    value: ItemListBits.empty()
  },
  reducers: {
    allClear: state => {
      state.value = state.value.setNoneImmut()
    },
    allSet: state => {
      state.value = state.value.setAllImmut()
    },
    bitClear: (state, action: PayloadAction<number>) => {
      state.value = state.value.clearBitImmut(action.payload)
    },
    bitSet: (state, action: PayloadAction<number>) => {
      state.value = state.value.setBitImmut(action.payload)
    },
    maskClear: (state, action: PayloadAction<ItemListBitMask>) => {
      state.value = state.value.applyMaskNotImmut(action.payload)
    },
    maskSet: (state, action: PayloadAction<ItemListBitMask>) => {
      state.value = state.value.applyMaskOrImmut(action.payload)
    },
    fromString: (state, action: PayloadAction<{value: string; length: number;}>) => {
      state.value = ItemListBits.fromString(action.payload.value, action.payload.length)
    },
    stateClear: state => {
      state.value = ItemListBits.empty()
    }
  }
})

export type ItemListSlice = ReturnType<typeof createItemListSlice>

/// Describes how to select an ItemListBits from the store's state, and references the corresponding Slice.
/// Needed if we are to have multiple ItemListBits instances using the same reducer (item pool, junk locations).
export interface ItemListStore {
  selector: (state: RootState) => ItemListBits
  slice: ItemListSlice
}
