import React from 'react';
import { Checkbox } from './Checkbox';
import { ItemPoolState } from './ConfigTypes';
import { setItemPoolColumnCount } from './RootStyle';
import './ItemPoolGrid.css';

const dataColumnAtt = 'data-column';
const dataRowAtt = 'data-row';
const dataHoveredColumnAtt = 'data-hovered-column';
const dataHoveredRowAtt = 'data-hovered-row';

interface ItemPoolGridProps {
  data: ItemPoolState;
  onStateChange?: () => void;
}

class ItemPoolGrid extends React.Component<ItemPoolGridProps> {
  gridRootElement: React.RefObject<HTMLDivElement>;

  constructor(props: ItemPoolGridProps) {
    super(props);
    this.gridRootElement = React.createRef<HTMLDivElement>();
  }

  triggerStateChange() {
    if (this.props.onStateChange)
      this.props.onStateChange();
  }

  /// Handle clicking the "all" checkbox.
  handleAllCheckboxClick = (event: React.MouseEvent<HTMLInputElement>) => {
    const checked = event.currentTarget.checked;
    if (checked) {
      this.props.data.list.setAll();
    } else {
      this.props.data.list.setNone();
    }
    this.forceUpdate();
    this.triggerStateChange();
  }

  /// Handle clicking a cell checkbox.
  handleCellCheckboxClick = (event: React.MouseEvent<HTMLInputElement>, colIndex: number, rowIndex: number) => {
    const checked = event.currentTarget.checked;
    const cell = this.props.data.repr.matrix.get(colIndex, rowIndex);
    this.props.data.applyBitOperation(checked, cell.bitMask);
    this.forceUpdate();
    this.triggerStateChange();
  };

  /// Handle clicking a column checkbox.
  handleColumnCheckboxClick = (event: React.MouseEvent<HTMLInputElement>, colIndex: number) => {
    const checked = event.currentTarget.checked;
    const column = this.props.data.repr.columns[colIndex];
    this.props.data.applyBitOperation(checked, column.bitMask);
    this.forceUpdate();
    this.triggerStateChange();
  };

  /// Handle clicking a row checkbox.
  handleRowCheckboxClick = (event: React.MouseEvent<HTMLInputElement>, rowIndex: number) => {
    const checked = event.currentTarget.checked;
    const row = this.props.data.repr.rows[rowIndex];
    this.props.data.applyBitOperation(checked, row.bitMask);
    this.forceUpdate();
    this.triggerStateChange();
  };

  handleMouseOver = (event: React.MouseEvent) => {
    if (this.gridRootElement.current) {
      this.updateHoveredAddress(event.currentTarget as HTMLElement);
    }
  };

  handleMouseOut = (event: React.MouseEvent) => {
    if (this.gridRootElement.current) {
      this.removeHoveredAddress();
    }
  };

  /// Clear grid root attributes for hover.
  removeHoveredAddress() {
    this.gridRootElement.current?.removeAttribute(dataHoveredColumnAtt);
    this.gridRootElement.current?.removeAttribute(dataHoveredRowAtt);
  }

  /// Update grid root attributes for which column and/or row is being hovered over.
  updateHoveredAddress(cellElement: HTMLElement) {
    const columnAttr = cellElement.getAttribute(dataColumnAtt);
    const rowAttr = cellElement.getAttribute(dataRowAtt);
    if (columnAttr) {
      this.gridRootElement.current?.setAttribute(
        dataHoveredColumnAtt,
        columnAttr
      );
    }
    if (rowAttr) {
      this.gridRootElement.current?.setAttribute(
        dataHoveredRowAtt,
        rowAttr
      );
    }
  };

  /// Update the CSS global variable for the item grid column count.
  updateColumnCountCSSVariable() {
    setItemPoolColumnCount(this.props.data.repr.columns.length);
  }

  renderGridCell(colIndex: number, rowIndex: number) {
    const checkedState = this.props.data.getCellCheckedState(colIndex, rowIndex);
    if (checkedState !== undefined) {
      return <Checkbox value={checkedState} onClick={(event) => this.handleCellCheckboxClick(event, colIndex, rowIndex)} />
    }
  }

  render() {
    this.updateColumnCountCSSVariable();
    const enabledCount = this.props.data.list.getEnabledCount();
    const totalCount = this.props.data.repr.items.length;
    const enabledPercentage = ((enabledCount / totalCount) * 100).toFixed(1);
    return (
      <>
        <div className="rando-itempool-root-container" ref={this.gridRootElement}>
          {/* Grid head. */}
          <div className="rando-itempool-head-container">
            <div className="rando-itempool-head-scroll-container">
              <div className="rando-itempool-head">
                {/* Show randomized percentage. */}
                <div className="rando-itempool-topleft">
                  <span>Randomized: {enabledCount} / {totalCount} ({enabledPercentage}%)</span>
                </div>

                {/* Column labels. */}
                {this.props.data.repr.columns.map(col => {
                  return (
                    <div className="rando-itempool-label-v" key={'label:' + col.index}>
                      <span>{col.data.Name}: +{col.count}</span>
                    </div>
                  )
                })}

                {/* "All" checkbox. */}
                <div className="rando-itempool-corner">
                  <div>
                    <Checkbox
                      value={this.props.data.list.getCheckedStateAll()}
                      onClick={this.handleAllCheckboxClick} />
                  </div>
                </div>

                {/* Column checkboxes. */}
                {this.props.data.repr.columns.map(col => {
                  return (
                    <div className="rando-itempool-col-checkbox" key={'checkbox:' + col.index}>
                      <Checkbox
                        value={this.props.data.getColumnCheckedState(col.index)}
                        onClick={(event) => this.handleColumnCheckboxClick(event, col.index)} />
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
          <div className="rando-itempool-body-container">
            <div className="rando-itempool-body-scroll-container">
              <div className="rando-itempool-body">
                {this.props.data.repr.rows.map(row => {
                  return (
                    <div className="rando-itempool-row" key={row.index}>
                      {/* Row label. */}
                      <div className="rando-itempool-label-h">
                        <label data-row={row.index} onMouseOver={this.handleMouseOver} onMouseOut={this.handleMouseOut}>
                          <span>{row.data.Name}: +{row.count}</span>
                          <Checkbox
                            value={this.props.data.getRowCheckedState(row.index)}
                            onClick={(event) => this.handleRowCheckboxClick(event, row.index)} />
                        </label>
                      </div>

                      {/* Grid cells. */}
                      {this.props.data.repr.columns.map(column => {
                        return (
                          <div
                            className="rando-itempool-cell"
                            data-column={column.index}
                            data-row={row.index}
                            key={row.index + ',' + column.index}
                            onMouseOver={this.handleMouseOver}
                            onMouseOut={this.handleMouseOut}
                          >
                            {this.renderGridCell(column.index, row.index)}
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
}

export default ItemPoolGrid;
