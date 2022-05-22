import React, { useEffect, useState } from 'react'
import { useDispatch } from 'react-redux'
import { api } from './common/Api'
import { ItemListRepr } from './common/ConfigTypes'
import { UserInterfaceJson } from './common/JsonTypes'
import itemPoolListSlice from './store/itemPoolListSlice'

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

  // Fetch generator-specific JSON file. For now, just gets item list representation.
  const fetchConfiguration = () => {
    api<UserInterfaceJson>('/ui.json').then((config) => {
      const grid = ItemListRepr.fromJson(config.ItemPool)
      setState({
        itemListRepr: grid
      })
      dispatch(itemPoolListSlice.actions.fromString({
        value: itemListTestString,
        length: grid.items.length
      }))
    })
  }

  useEffect(() => {
    fetchConfiguration()
    return () => {
      // Cleanup state & store.
      setState({
        itemListRepr: undefined
      })
      dispatch(itemPoolListSlice.actions.stateClear())
    }
  }, [])

  return (
    <ItemListReprContext.Provider value={state?.itemListRepr}>
      {props.children}
    </ItemListReprContext.Provider>
  )
}

export default Randomizer
