import React, { useState } from 'react'
import { Checkbox } from './Checkbox'
import { useItemListRepr } from './Randomizer'
import { CheckedState } from './common/CheckedState'
import { ItemRepr } from './common/ConfigTypes'
import { CategoryGroup, CategoryType, ItemGroup } from './common/ItemCategory'
import { useAppDispatch, useAppSelector } from './hooks'
import { ItemListStore } from './store/createItemListSlice'

interface CardProps {
  itemRepr: ItemRepr
  store: ItemListStore
}

const Card = (props: CardProps) => {
  const index = props.itemRepr.index
  const selected = useAppSelector(state => props.store.selector(state).hasBit(index))
  const checkedState = CheckedState.fromBoolean(selected)
  const dispatch = useAppDispatch()

  const onChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const checked = event.currentTarget.checked;
    if (checked) {
      dispatch(props.store.slice.actions.bitSet(index))
    } else {
      dispatch(props.store.slice.actions.bitClear(index))
    }
  }

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
          onChange={onChange}
        />
        <p className="inline-block text-ellipsis">{props.itemRepr.locationName}</p>
      </label>
    </li>
  )
}

interface ItemQueryCategoryProps {
  name: string;
  itemGroup: ItemGroup;
  store: ItemListStore;
}

const ItemQueryCategory = (props: ItemQueryCategoryProps) => {
  const bitMask = props.itemGroup.bitMask
  const checkedState = useAppSelector(state => props.store.selector(state).getCheckedState(bitMask))
  const dispatch = useAppDispatch()

  const onChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const checked = event.currentTarget.checked
    if (checked) {
      dispatch(props.store.slice.actions.maskSet(bitMask))
    } else {
      dispatch(props.store.slice.actions.maskClear(bitMask))
    }
  }

  return (
    <div className="mb-12 relative">
      <div
        className="absolute border-4 border-b-0 border-neutral-500 border-double w-full h-4 -z-10"
        style={{ top: '1rem' }}
      ></div>
      <div className="flex flex-row items-center my-4">
        <label className="my-app-bg w-max px-2 flex flex-row gap-2 items-center ml-4 cursor-pointer">
          <p className="font-semibold text-3xl">{props.name}</p>
          <Checkbox
            className="h-4 w-4 shrink-0 cursor-pointer"
            value={checkedState}
            onChange={onChange}
          />
        </label>
      </div>
      <ul className="gap-4 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
        {props.itemGroup.items.map((x) => (
          <Card
            key={x.locationName}
            itemRepr={x}
            store={props.store}
          />
        ))}
      </ul>
    </div>
  )
}

interface ItemQueryViewState {
  categoryType: CategoryType;
  queryResult: CategoryGroup;
  queryWords?: string[];
}

interface ItemQueryViewProps {
  store: ItemListStore
}

const ItemQueryView = (props: ItemQueryViewProps) => {
  const defaultCategoryType = CategoryType.Location;
  const itemListRepr = useItemListRepr();
  const [state, setState] = useState<ItemQueryViewState>({
    categoryType: defaultCategoryType,
    queryResult: itemListRepr.categoryGroups.byCategory(defaultCategoryType)
  })

  const updateQueryResult = (categoryType: CategoryType, queryWords?: string[]) => {
    const defaultCategoryGroups = itemListRepr.categoryGroups.byCategory(categoryType)
    if (queryWords === undefined || queryWords.length === 0) {
      setState({
        categoryType,
        queryResult: defaultCategoryGroups,
        queryWords: undefined,
      })
    } else {
      const queryResult = defaultCategoryGroups.query(queryWords)
      setState({
        categoryType,
        queryResult,
        queryWords,
      })
    }
  }

  const onCategorySelect = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const selectedOption = event.currentTarget.selectedOptions[0];
    const categoryType = Number.parseInt(selectedOption.value) as CategoryType;
    if (state.categoryType !== categoryType) {
      updateQueryResult(categoryType, state.queryWords);
    }
  }

  const onQueryChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const text = event.currentTarget.value;
    const queryWords = text.split(/\s/g);
    updateQueryResult(state.categoryType, queryWords);
  }

  return (
    <div className="p-4 w-full">
      <div className="flex flex-col md:flex-row gap-4 place-content-center p-4">
        <input className="w-full md:w-1/2" type="text" placeholder="Query" onChange={onQueryChange} />
        <div className="flex flex-row gap-2 items-center justify-center">
          <label className="md:hidden" htmlFor="sort-by">
            Categories:
          </label>
          <div className="select">
            <select className="leading-4" id="sort-by" onChange={onCategorySelect}>
              <option value={CategoryType.Location}>Location</option>
              <option value={CategoryType.Item}>Item</option>
            </select>
            <span className="focus"></span>
          </div>
        </div>
      </div>
      <div className="p-4 w-full select-none">
        {Array.from(state.queryResult.childrenSorted(), ([categoryValue, itemGroup]) => {
          const categoryValueName = categoryValue ?? '<None>';
          return (
            <ItemQueryCategory
              key={categoryValue}
              name={categoryValueName}
              itemGroup={itemGroup}
              store={props.store}
            />
          );
        })}
      </div>
    </div>
  );
}

export default ItemQueryView;
