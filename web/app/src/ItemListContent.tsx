import React from 'react';
import { ItemPoolState } from './ConfigTypes';

import Slider from './Slider';
import ItemQueryView from './ItemQueryView';
import ItemPoolGrid from './ItemPoolGrid';

enum ViewState {
  Query,
  Matrix,
}

class ItemListContentState {
  viewState: ViewState = ViewState.Query;
}

interface ItemListContentProps {
  title: string;
  data: ItemPoolState;
  allowMatrix?: boolean;
}

class ItemListContent extends React.Component<ItemListContentProps, ItemListContentState> {
  itemListTextElementRef: React.RefObject<HTMLInputElement>;

  constructor(props: ItemListContentProps) {
    super(props);
    this.state = {
      viewState: ViewState.Query,
    };
    this.itemListTextElementRef = React.createRef();
  }

  onStateChange = () => {
    if (this.itemListTextElementRef.current !== null) {
      this.itemListTextElementRef.current.value = this.props.data.list.toString();
    }
  };

  onViewCheckboxChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const checked = event.currentTarget.checked;
    this.setState({
      viewState: checked ? ViewState.Matrix : ViewState.Query,
    });
  };

  renderView() {
    if (this.state.viewState === ViewState.Query) {
      return <ItemQueryView data={this.props.data} onStateChange={this.onStateChange} />;
    } else if (this.state.viewState === ViewState.Matrix) {
      return <ItemPoolGrid data={this.props.data} onStateChange={this.onStateChange} />;
    }
  }

  render() {
    return (
      <>
        <div className="flex flex-row-reverse gap-4 items-center px-3 py-1">
          <div className={`hidden xl:flex flex-row gap-2 items-center ${this.props.allowMatrix ? '' : 'hidden'}`}>
            <span className="font-semibold">Matrix View</span>
            <Slider onChange={this.onViewCheckboxChange} />
          </div>
          <div className="grow h-1 bg-[#404040]"></div>
          <h1 className="font-semibold text-2xl pl-2">{this.props.title}</h1>
        </div>
        <div className="flex justify-center p-3">
          <input
            className="font-mono w-full"
            placeholder="Item Pool String"
            type="text"
            ref={this.itemListTextElementRef}
            readOnly
            value={this.props.data.list.toString()}
          />
        </div>
        <div>{this.renderView()}</div>
      </>
    );
  }
}

export default ItemListContent;
