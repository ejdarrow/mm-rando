import { configureStore } from '@reduxjs/toolkit'
import itemPoolListSlice from './store/itemPoolListSlice'
import junkLocationsListSlice from './store/junkLocationsListSlice'

const store = configureStore({
  reducer: {
    itemPool: itemPoolListSlice.reducer,
    junkLocations: junkLocationsListSlice.reducer,
  }
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

export default store
