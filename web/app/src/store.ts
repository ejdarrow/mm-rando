import { configureStore } from '@reduxjs/toolkit'
import itemPoolListSlice from './store/itemPoolListSlice'

const store = configureStore({
  reducer: {
    itemPool: itemPoolListSlice.reducer,
  }
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

export default store
