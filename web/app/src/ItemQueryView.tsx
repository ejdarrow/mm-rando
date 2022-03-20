import React from 'react';
import { ItemPoolItemRepr, ItemPoolState } from './ConfigTypes';
import { CategoryGroup, CategoryType, ItemGroup } from './ItemCategory';
import { ItemListBits } from './ItemList';
import { CheckedState, Checkbox } from './Checkbox';

interface CardProps {
  itemListBits: ItemListBits;
  itemRepr: ItemPoolItemRepr;
  onChange: (event: React.ChangeEvent<HTMLInputElement>, itemCard: Card) => void;
}

class Card extends React.Component<CardProps> {
  render() {
    const selected = this.props.itemListBits.hasBit(this.props.itemRepr.index);
    const checkedState = CheckedState.fromBoolean(selected);
    return (
      <li>
        <label
          className={`
            h-16 lg:h-20 flex flex-row items-center gap-3 px-2 text-sm font-semibold
            border-2 cursor-pointer rounded-md
            ${
              selected
                ? 'border-violet-500 outline outline-2 outline-violet-500 text-neutral-400'
                : 'border-neutral-500 text-neutral-500'
            }`}
        >
          <Checkbox
            className="cursor-pointer h-5 w-5 shrink-0"
            value={checkedState}
            onChange={(event) => this.props.onChange(event, this)}
          />
          <p className="inline-block text-ellipsis">{this.props.itemRepr.locationName}</p>
        </label>
      </li>
    );
  }
}

interface ItemQueryCategoryProps {
  name: string;
  itemGroup: ItemGroup;
  itemListBits: ItemListBits;
  onCategoryCheckboxChange: (event: React.ChangeEvent<HTMLInputElement>, categoryComponent: ItemQueryCategory) => void;
  onItemCheckboxChange: (
    event: React.ChangeEvent<HTMLInputElement>,
    categoryComponent: ItemQueryCategory,
    itemCard: Card
  ) => void;
}

class ItemQueryCategory extends React.Component<ItemQueryCategoryProps> {
  checkboxRef: React.RefObject<HTMLInputElement>;

  constructor(props: ItemQueryCategoryProps) {
    super(props);
    this.checkboxRef = React.createRef();
  }

  onItemCheckboxChange = (event: React.ChangeEvent<HTMLInputElement>, itemCard: Card) => {
    return this.props.onItemCheckboxChange(event, this, itemCard);
  };

  getCheckedState() {
    return this.props.itemListBits.getCheckedState(this.props.itemGroup.bitMask);
  }

  updateCheckbox() {
    const checkedState = this.getCheckedState();
    const checkboxElement = this.checkboxRef.current as HTMLInputElement;
    CheckedState.updateCheckbox(checkboxElement, checkedState);
  }

  render() {
    return (
      <div className="mb-12 relative">
        <div
          className="absolute border-4 border-b-0 border-neutral-500 border-double w-full h-4 -z-10"
          style={{ top: '1rem' }}
        ></div>
        <div className="flex flex-row items-center my-4">
          <label className="my-app-bg w-max px-2 flex flex-row gap-2 items-center ml-4 cursor-pointer">
            <p className="font-semibold text-3xl">{this.props.name}</p>
            <Checkbox
              inputRef={this.checkboxRef}
              className="h-4 w-4 shrink-0 cursor-pointer"
              value={this.props.itemListBits.getCheckedState(this.props.itemGroup.bitMask)}
              onChange={(event) => this.props.onCategoryCheckboxChange(event, this)}
            />
          </label>
        </div>
        <ul className="gap-4 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {this.props.itemGroup.items.map((x) => (
            <Card
              key={x.locationName}
              itemListBits={this.props.itemListBits}
              itemRepr={x}
              onChange={this.onItemCheckboxChange}
            />
          ))}
        </ul>
      </div>
    );
  }
}

interface ItemQueryViewState {
  categoryType: CategoryType;
  queryResult: CategoryGroup;
  queryWords?: string[];
}

interface ItemQueryViewProps {
  data: ItemPoolState;
  onStateChange?: () => void;
}

class ItemQueryView extends React.Component<ItemQueryViewProps, ItemQueryViewState> {
  constructor(props: ItemQueryViewProps) {
    super(props);
    const categoryType = CategoryType.Location;
    this.state = {
      categoryType,
      queryResult: props.data.repr.categoryGroups.byCategory(categoryType),
    };
  }

  /// Callback when a category checkbox has changed.
  onCategoryCheckboxChange = (event: React.ChangeEvent<HTMLInputElement>, categoryComponent: ItemQueryCategory) => {
    const checked = event.currentTarget.checked;
    const itemGroup = categoryComponent.props.itemGroup;
    this.props.data.applyBitOperation(checked, itemGroup.bitMask);
    this.updateSideEffects();
    categoryComponent.forceUpdate();
    this.triggerStateChange();
  };

  /// Callback when an individual item checkbox has changed.
  onItemCheckboxChange = (
    event: React.ChangeEvent<HTMLInputElement>,
    categoryComponent: ItemQueryCategory,
    itemCard: Card
  ) => {
    const checked = event.currentTarget.checked;
    const itemRepr = itemCard.props.itemRepr;
    this.props.data.list.modifyBit(checked, itemRepr.index);
    this.updateSideEffects();
    categoryComponent.updateCheckbox();
    itemCard.forceUpdate();
    this.triggerStateChange();
  };

  triggerStateChange() {
    if (this.props.onStateChange) this.props.onStateChange();
  }

  onCategorySelect = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const selectedOption = event.currentTarget.selectedOptions[0];
    const categoryType = Number.parseInt(selectedOption.value) as CategoryType;
    if (this.state.categoryType !== categoryType) {
      this.updateQueryResult(categoryType, this.state.queryWords);
    }
  };

  onQueryChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const text = event.currentTarget.value;
    const queryWords = text.split(/\s/g);
    this.updateQueryResult(this.state.categoryType, queryWords);
  };

  updateSideEffects() {
    this.triggerStateChange();
  }

  updateQueryResult(categoryType: CategoryType, queryWords?: string[]) {
    const defaultCategoryGroups = this.props.data.repr.categoryGroups.byCategory(categoryType);
    if (queryWords === undefined || queryWords.length === 0) {
      this.setState({
        categoryType,
        queryResult: defaultCategoryGroups,
        queryWords: undefined,
      });
    } else {
      const queryResult = defaultCategoryGroups.query(queryWords);
      this.setState({
        categoryType,
        queryResult,
        queryWords,
      });
    }
  }

  render() {
    return (
      <div className="p-4 w-full">
        <div className="flex flex-col md:flex-row gap-4 place-content-center p-4">
          <input className="w-full md:w-1/2" type="text" placeholder="Query" onChange={this.onQueryChange} />
          <div className="flex flex-row gap-2 items-center justify-center">
            <label className="md:hidden" htmlFor="sort-by">
              Categories:
            </label>
            <div className="select">
              <select className="leading-4" id="sort-by" onChange={this.onCategorySelect}>
                <option value={CategoryType.Location}>Location</option>
                <option value={CategoryType.Item}>Item</option>
              </select>
              <span className="focus"></span>
            </div>
          </div>
        </div>
        <div className="p-4 w-full select-none">
          {Array.from(this.state.queryResult.childrenSorted(), ([categoryValue, itemGroup]) => {
            const categoryValueName = categoryValue ?? '<None>';
            return (
              <ItemQueryCategory
                key={categoryValue}
                name={categoryValueName}
                itemGroup={itemGroup}
                itemListBits={this.props.data.list}
                onCategoryCheckboxChange={this.onCategoryCheckboxChange}
                onItemCheckboxChange={this.onItemCheckboxChange}
              />
            );
          })}
        </div>
      </div>
    );
  }
}

export default ItemQueryView;
