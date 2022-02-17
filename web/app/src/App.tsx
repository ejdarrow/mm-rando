import React from 'react';
import { api } from './Api';
import { ItemPoolGridRepr, ItemPoolState } from './ConfigTypes';
import { ItemListBits } from './ItemList';
import { UserInterfaceJson } from './JsonTypes';

import ItemPoolGrid from './ItemPoolGrid';
import HUDScreen from './HUDScreen';
import Nav from './Nav';

import './App.css';

// Test string.
const itemListTestString = '--------------------40c-80000000----21ffff-ffffffff-ffffffff-f0000000-7bbeeffa-7fffffff-e6f1fffe-ffffffff';

interface AppProps {
}

interface AppState {
  selectedIdentifier: string,
  itemPool?: ItemPoolState,
}

class App extends React.Component<AppProps, AppState> {
  constructor(props: AppProps) {
    super(props);
    this.state = {
      selectedIdentifier: 'randomizer',
    };
  }

  componentDidMount() {
    this.fetchConfiguration();
  }

  fetchConfiguration() {
    api<UserInterfaceJson>('/ui.json').then(config => {
      const grid = ItemPoolGridRepr.fromJson(config.ItemPool);
      const list = ItemListBits.fromString(itemListTestString, grid.items.length);
      const state = new ItemPoolState(grid, list);
      this.setState({
        itemPool: state,
      });
    });
  }

  handleNavItemClick = (event: React.MouseEvent<HTMLAnchorElement>, identifier: string) => {
    this.setState({
      selectedIdentifier: identifier,
    })
  }

  handleHUDScreenClick = (event: React.MouseEvent<HTMLElement>, identifier: string) => {
    // TODO
  }

  renderRightMostContent() {
    if (this.state.selectedIdentifier == 'hud colors') {
      return <HUDScreen onClick={this.handleHUDScreenClick} />
    }

    if (this.state.selectedIdentifier == 'item pool' && this.state.itemPool !== undefined) {
      return <ItemPoolGrid data={this.state.itemPool} />
    }
  }

  render() {
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
          <div className="pt-6 hidden lg:block fixed z-10 inset-0 top-[3.8125rem] left-[max(0px,calc(50%-45rem))] right-auto w-[18rem] pb-10 px-2 overflow-y-auto" id="nav">
            <Nav onNavItemClick={this.handleNavItemClick} selected={this.state.selectedIdentifier} />
          </div>
          <div className="flex flex-col mx-auto h-full max-w-8xl lg:pl-[18rem] pt-4">
            {this.renderRightMostContent()}
          </div>
        </div>
      </>
    )
  }
}

export default App;
