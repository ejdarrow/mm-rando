import React, { useEffect, useState } from 'react'
import { useDispatch } from 'react-redux'
import { api } from '../../common/Api'
import { ItemListRepr } from '../../common/ConfigTypes'
import { UserInterfaceJson } from '../../common/JsonTypes'
import itemPoolListSlice from '../../store/itemPoolListSlice'
import junkLocationsListSlice from '../../store/junkLocationsListSlice'

const ItemListReprContext = React.createContext<ItemListRepr | undefined>(undefined)

export const useContextOrError = <T,>(context: React.Context<T | undefined>): T => {
  const result = React.useContext(context)
  if (result === undefined) {
    throw new Error('Retrieved React.Context should not be undefined')
  }
  return result
}

export const useItemListRepr = () => useContextOrError(ItemListReprContext)

// Test string.
const itemListTestString =
  '--------------------40c-80000000----21ffff-ffffffff-ffffffff-f0000000-7bbeeffa-7fffffff-e6f1fffe-ffffffff'

interface RandomizerProps {}

class RandomizerState {
  itemListRepr?: ItemListRepr
}

const Randomizer = (props: React.PropsWithChildren<RandomizerProps>) => {
  const [state, setState] = useState<RandomizerState>()
  const dispatch = useDispatch()

  useEffect(() => {
    // Fetch generator-specific JSON file. For now, just gets item list representation.
    api<UserInterfaceJson>('/ui.json').then((config) => {
      const grid = ItemListRepr.fromJson(config.ItemPool)
      const itemCount = grid.items.length
      setState({
        itemListRepr: grid
      })
      dispatch(itemPoolListSlice.actions.fromString({
        value: itemListTestString,
        length: itemCount
      }))
      dispatch(junkLocationsListSlice.actions.withLength(itemCount))
    })
    return () => {
      // Cleanup state & store.
      setState({
        itemListRepr: undefined
      })
      dispatch(itemPoolListSlice.actions.stateClear())
      dispatch(junkLocationsListSlice.actions.stateClear())
    }
  }, [dispatch])

  return (
    <ItemListReprContext.Provider value={state?.itemListRepr}>
      {props.children}
    </ItemListReprContext.Provider>
  )
}

export default Randomizer
