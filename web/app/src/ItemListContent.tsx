import React, { useState } from 'react'
import ItemListTextInput from './ItemListTextInput'
import ItemMatrixView from './ItemMatrixView'
import ItemQueryView from './ItemQueryView'
import Slider from './Slider'
import { RootState } from './store'
import { ItemListStore } from './store/createItemListSlice'

enum ViewState {
  Query,
  Matrix
}

class ItemListContentState {
  viewState: ViewState = ViewState.Query
}

interface ItemListContentProps {
  title: string
  allowMatrix?: boolean
  store: ItemListStore
}

const ItemListContent = (props: ItemListContentProps) => {
  const [state, setState] = useState<ItemListContentState>(new ItemListContentState())

  const onViewCheckboxChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const checked = event.currentTarget.checked
    setState({
      viewState: checked ? ViewState.Matrix : ViewState.Query
    })
  }

  const renderView = () => {
    if (state.viewState === ViewState.Query) {
      return <ItemQueryView store={props.store} />
    } else if (state.viewState === ViewState.Matrix) {
      return <ItemMatrixView store={props.store} />
    }
  }

  return (
    <>
      <div className="flex flex-row-reverse gap-4 items-center px-3 py-1">
        <div className={`hidden xl:flex flex-row gap-2 items-center ${props.allowMatrix ? '' : 'hidden'}`}>
          <span className="font-semibold">Matrix View</span>
          <Slider onChange={onViewCheckboxChange} />
        </div>
        <div className="grow h-1 bg-[#404040]"></div>
        <h1 className="font-semibold text-2xl pl-2">{props.title}</h1>
      </div>
      <div className="flex justify-center p-3">
        <ItemListTextInput selector={(state: RootState) => state.itemPool.value.toString()}/>
      </div>
      <div>{renderView()}</div>
    </>
  )
}

export default ItemListContent
