import React, { useRef } from 'react'
import { Checkbox } from './Checkbox'
import { useItemListRepr } from './Randomizer'
import { setItemPoolColumnCount } from './RootStyle'
import { ItemListBitMask, getCellCheckedState, getColumnCheckedState, getRowCheckedState } from './common/ItemList'
import { useAppDispatch, useAppSelector } from './hooks'
import { ItemListStore } from './store/createItemListSlice'
import './ItemMatrixView.css'

const dataColumnAtt = 'data-column';
const dataRowAtt = 'data-row';
const dataHoveredColumnAtt = 'data-hovered-column';
const dataHoveredRowAtt = 'data-hovered-row';

interface ItemMatrixViewProps {
  store: ItemListStore
}

const ItemMatrixView = (props: ItemMatrixViewProps) => {
  const gridRootElement = useRef<HTMLDivElement>(null)
  const itemListRepr = useItemListRepr()
  const itemListBits = useAppSelector(state => props.store.selector(state))
  const dispatch = useAppDispatch()

  const dispatchBitMaskOperation = (operation: boolean, bitMask: ItemListBitMask) => {
    if (operation) {
      dispatch(props.store.slice.actions.maskSet(bitMask))
    } else {
      dispatch(props.store.slice.actions.maskClear(bitMask))
    }
  }

  /// Handle clicking the "all" checkbox.
  const handleAllCheckboxClick = (event: React.MouseEvent<HTMLInputElement>) => {
    const checked = event.currentTarget.checked
    if (checked) {
      dispatch(props.store.slice.actions.allSet())
    } else {
      dispatch(props.store.slice.actions.allClear())
    }
  }

  /// Handle clicking a cell checkbox.
  const handleCellCheckboxClick = (event: React.MouseEvent<HTMLInputElement>, colIndex: number, rowIndex: number) => {
    const checked = event.currentTarget.checked
    const cell = itemListRepr.matrix.get(colIndex, rowIndex)
    dispatchBitMaskOperation(checked, cell.bitMask)
  }

  /// Handle clicking a column checkbox.
  const handleColumnCheckboxClick = (event: React.MouseEvent<HTMLInputElement>, colIndex: number) => {
    const checked = event.currentTarget.checked
    const column = itemListRepr.columns[colIndex]
    dispatchBitMaskOperation(checked, column.bitMask)
  }

  /// Handle clicking a row checkbox.
  const handleRowCheckboxClick = (event: React.MouseEvent<HTMLInputElement>, rowIndex: number) => {
    const checked = event.currentTarget.checked
    const row = itemListRepr.rows[rowIndex]
    dispatchBitMaskOperation(checked, row.bitMask)
  }

  const handleMouseOver = (event: React.MouseEvent) => {
    if (gridRootElement.current) {
      updateHoveredAddress(event.currentTarget as HTMLElement)
    }
  }

  const handleMouseOut = (event: React.MouseEvent) => {
    if (gridRootElement.current) {
      removeHoveredAddress()
    }
  }

  /// Clear grid root attributes for hover.
  const removeHoveredAddress = () => {
    gridRootElement.current?.removeAttribute(dataHoveredColumnAtt)
    gridRootElement.current?.removeAttribute(dataHoveredRowAtt)
  }

  /// Update grid root attributes for which column and/or row is being hovered over.
  const updateHoveredAddress = (cellElement: HTMLElement) => {
    const columnAttr = cellElement.getAttribute(dataColumnAtt)
    const rowAttr = cellElement.getAttribute(dataRowAtt)
    if (columnAttr) {
      gridRootElement.current?.setAttribute(dataHoveredColumnAtt, columnAttr)
    }
    if (rowAttr) {
      gridRootElement.current?.setAttribute(dataHoveredRowAtt, rowAttr)
    }
  }

  /// Update the CSS global variable for the item grid column count.
  const updateColumnCountCSSVariable = () => {
    setItemPoolColumnCount(itemListRepr.columns.length)
  }

  const renderGridCell = (colIndex: number, rowIndex: number) => {
    const checkedState = getCellCheckedState(itemListBits, itemListRepr, colIndex, rowIndex)
    if (checkedState !== undefined) {
      return (
        <Checkbox value={checkedState} onClick={(event) => handleCellCheckboxClick(event, colIndex, rowIndex)} />
      )
    }
  }

  // TODO: Place in effect?
  updateColumnCountCSSVariable()

  const enabledCount = itemListBits.getEnabledCount();
  const totalCount = itemListRepr.items.length;
  const enabledPercentage = ((enabledCount / totalCount) * 100).toFixed(1);
  return (
    <>
      <div className="rando-itempool-root-container" ref={gridRootElement}>
        {/* Grid head. */}
        <div className="rando-itempool-head-container">
          <div className="rando-itempool-head-scroll-container">
            <div className="rando-itempool-head">
              {/* Show randomized percentage. */}
              <div className="rando-itempool-topleft">
                <span>
                  Randomized: {enabledCount} / {totalCount} ({enabledPercentage}%)
                </span>
              </div>

              {/* Column labels. */}
              {itemListRepr.columns.map((col) => {
                return (
                  <div className="rando-itempool-label-v" key={'label:' + col.index}>
                    <span>
                      {col.data.Name}: +{col.count}
                    </span>
                  </div>
                );
              })}

              {/* "All" checkbox. */}
              <div className="rando-itempool-corner">
                <div>
                  <Checkbox value={itemListBits.getCheckedStateAll()} onClick={handleAllCheckboxClick} />
                </div>
              </div>

              {/* Column checkboxes. */}
              {itemListRepr.columns.map((col) => {
                return (
                  <div className="rando-itempool-col-checkbox" key={'checkbox:' + col.index}>
                    <Checkbox
                      value={getColumnCheckedState(itemListBits, itemListRepr, col.index)}
                      onClick={(event) => handleColumnCheckboxClick(event, col.index)}
                    />
                  </div>
                );
              })}
            </div>
            <div></div>
            <div className="hidden-vertical-scrollbar"></div>
          </div>
          <div></div>
          <div></div>
        </div>

        {/* Grid body. */}
        <div className="rando-itempool-body-container">
          <div className="rando-itempool-body-scroll-container">
            <div className="rando-itempool-body">
              {itemListRepr.rows.map((row) => {
                return (
                  <div className="rando-itempool-row" key={row.index}>
                    {/* Row label. */}
                    <div className="rando-itempool-label-h">
                      <label data-row={row.index} onMouseOver={handleMouseOver} onMouseOut={handleMouseOut}>
                        <span>
                          {row.data.Name}: +{row.count}
                        </span>
                        <Checkbox
                          value={getRowCheckedState(itemListBits, itemListRepr, row.index)}
                          onClick={(event) => handleRowCheckboxClick(event, row.index)}
                        />
                      </label>
                    </div>

                    {/* Grid cells. */}
                    {itemListRepr.columns.map((column) => {
                      return (
                        <div
                          className="rando-itempool-cell"
                          data-column={column.index}
                          data-row={row.index}
                          key={row.index + ',' + column.index}
                          onMouseOver={handleMouseOver}
                          onMouseOut={handleMouseOut}
                        >
                          {renderGridCell(column.index, row.index)}
                        </div>
                      );
                    })}
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      </div>
    </>
  )
}

export default ItemMatrixView;
