import React, { useRef } from 'react'
import { Checkbox } from '../common/Checkbox'
import { useItemListRepr } from './Randomizer'
import { rootStyle } from '../../common/RootStyle'
import { ItemListBitMask } from '../../common/ItemList'
import { useAppDispatch, useAppSelector } from '../../common/hooks'
import { ItemListStore } from '../../store/createItemListSlice'
import styles from './styles/ItemMatrixView.module.css'

const dataColumnAtt = 'data-column'
const dataRowAtt = 'data-row'
const dataHoveredColumnAtt = 'data-hovered-column'
const dataHoveredRowAtt = 'data-hovered-row'

interface ItemListCounterProps {
  store: ItemListStore
}

const setItemPoolColumnCount = (count: number) => {
  rootStyle().setProperty('--app-itempool-column-count', count.toString());
};

const ItemListCounter = (props: ItemListCounterProps) => {
  const enabledCount = useAppSelector(state => props.store.selector(state).getEnabledCount())
  const itemListRepr = useItemListRepr()
  const totalCount = itemListRepr.items.length
  const enabledPercentage = ((enabledCount / totalCount) * 100).toFixed(1)

  return (
    <span>
      Randomized: {enabledCount} / {totalCount} ({enabledPercentage}%)
    </span>
  )
}

interface IdentityCheckboxProps {
  store: ItemListStore
}

const IdentityCheckbox = (props: IdentityCheckboxProps) => {
  const checkedState = useAppSelector(state => props.store.selector(state).getCheckedStateAll())
  const dispatch = useAppDispatch()

  const onClick = (event: React.MouseEvent<HTMLInputElement>) => {
    const checked = event.currentTarget.checked
    if (checked) {
      dispatch(props.store.slice.actions.allSet())
    } else {
      dispatch(props.store.slice.actions.allClear())
    }
  }

  return <Checkbox value={checkedState} onClick={onClick} />
}

interface MaskCheckboxProps {
  bitMask: ItemListBitMask
  store: ItemListStore
}

const MaskCheckbox = (props: MaskCheckboxProps) => {
  const checkedState = useAppSelector(state => props.store.selector(state).getCheckedState(props.bitMask))
  const dispatch = useAppDispatch()

  const onClick = (event: React.MouseEvent<HTMLInputElement>) => {
    const checked = event.currentTarget.checked
    const bitMask = props.bitMask
    if (checked) {
      dispatch(props.store.slice.actions.maskSet(bitMask))
    } else {
      dispatch(props.store.slice.actions.maskClear(bitMask))
    }
  }

  return <Checkbox value={checkedState} onClick={onClick} />
}

interface ItemMatrixViewProps {
  store: ItemListStore
}

const ItemMatrixView = (props: ItemMatrixViewProps) => {
  const gridRootElement = useRef<HTMLDivElement>(null)
  const itemListRepr = useItemListRepr()

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
    if (itemListRepr.hasCell(colIndex, rowIndex)) {
      const data = itemListRepr.matrix.get(colIndex, rowIndex)
      return <MaskCheckbox store={props.store} bitMask={data.bitMask} />
    }
  }

  // TODO: Place in effect?
  updateColumnCountCSSVariable()

  return (
    <>
      <div className={styles['root-container']} ref={gridRootElement}>
        {/* Grid head. */}
        <div className={styles['head-container']}>
          <div className={styles['head-scroll-container']}>
            <div className={styles['head']}>
              {/* Show randomized percentage. */}
              <div className={styles['topleft']}>
                <ItemListCounter store={props.store} />
              </div>

              {/* Column labels. */}
              {itemListRepr.columns.map((col) => {
                return (
                  <div className={styles['label-v']} key={'label:' + col.index}>
                    <span>
                      {col.data.Name}: +{col.count}
                    </span>
                  </div>
                )
              })}

              {/* "All" checkbox. */}
              <div className={styles['corner']}>
                <div>
                  <IdentityCheckbox store={props.store} />
                </div>
              </div>

              {/* Column checkboxes. */}
              {itemListRepr.columns.map((col) => {
                return (
                  <div key={'checkbox:' + col.index}>
                    <MaskCheckbox store={props.store} bitMask={col.bitMask} />
                  </div>
                )
              })}
            </div>
            <div></div>
            <div className="hidden-vertical-scrollbar"></div>
          </div>
          <div></div>
          <div></div>
        </div>

        {/* Grid body. */}
        <div className={styles['body-container']}>
          <div className={styles['body-scroll-container']}>
            <div className={styles['body']}>
              {itemListRepr.rows.map((row) => {
                return (
                  <div className={styles['row']} key={row.index}>
                    {/* Row label. */}
                    <div className={styles['label-h']}>
                      <label data-row={row.index} onMouseOver={handleMouseOver} onMouseOut={handleMouseOut}>
                        <span>
                          {row.data.Name}: +{row.count}
                        </span>
                        <MaskCheckbox store={props.store} bitMask={row.bitMask} />
                      </label>
                    </div>

                    {/* Grid cells. */}
                    {itemListRepr.columns.map((column) => {
                      return (
                        <div
                          className={styles['cell']}
                          data-column={column.index}
                          data-row={row.index}
                          key={row.index + ',' + column.index}
                          onMouseOver={handleMouseOver}
                          onMouseOut={handleMouseOut}
                        >
                          {renderGridCell(column.index, row.index)}
                        </div>
                      )
                    })}
                  </div>
                )
              })}
            </div>
          </div>
        </div>
      </div>
    </>
  )
  
}

export default ItemMatrixView
