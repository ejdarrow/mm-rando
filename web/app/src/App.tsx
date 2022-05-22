import React, { useState } from 'react'
import HUDScreen from './HUDScreen'
import ItemListContent from './ItemListContent'
import Nav from './Nav'
import Randomizer from './Randomizer'
import { itemPoolListStore } from './store/itemPoolListSlice'
import { junkLocationsListStore } from './store/junkLocationsListSlice'
import './App.css'

interface AppProps {}

interface AppState {
  selectedIdentifier: string
}

const App = (props: AppProps) => {
  const [state, setState] = useState<AppState>({
    selectedIdentifier: 'randomizer'
  })

  const handleNavItemClick = (event: React.MouseEvent<HTMLButtonElement>, identifier: string) => {
    setState({
      selectedIdentifier: identifier
    })
  }

  const handleHUDScreenClick = (event: React.MouseEvent<HTMLElement>, identifier: string) => {
    // TODO
  }

  const renderRightMostContent = () => {
    if (state.selectedIdentifier === 'hud colors') {
      return <HUDScreen onClick={handleHUDScreenClick} />
    }

    if (state.selectedIdentifier === 'item pool') {
      return <ItemListContent title="Item Pool" allowMatrix={true} store={itemPoolListStore} />
    }

    if (state.selectedIdentifier === 'junk locations') {
      return <ItemListContent title='Junk Locations' allowMatrix={false} store={junkLocationsListStore} />
    }
  }

  return (
    <>
      <div className="sticky top-0 z-40 w-full backdrop-blur flex-none border-b border-[#180030] bg-[#0c0018]/75">
        <div className="max-w-8xl mx-auto">
          <div className="py-4 lg:px-2 dark:border-slate-300/10 mx-4 lg:mx-0">
            <span>Majora's Mask Randomizer</span>
          </div>
        </div>
      </div>
      <div className="pt-2">
        <div
          className="pt-6 hidden lg:block fixed z-10 inset-0 top-[3.8125rem] left-[max(0px,calc(50%-45rem))] right-auto w-[18rem] pb-10 px-2 overflow-y-auto"
          id="nav"
        >
          <Nav onNavItemClick={handleNavItemClick} selected={state.selectedIdentifier} />
        </div>
        <div className="flex flex-col mx-auto h-full max-w-8xl lg:pl-[18rem] pt-4">
          <Randomizer>
            {renderRightMostContent()}
          </Randomizer>
        </div>
      </div>
    </>
  )
}

export default App
