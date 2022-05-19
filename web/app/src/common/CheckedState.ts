export enum CheckedState {
  Unchecked,
  Checked,
  Indeterminate,
}

export namespace CheckedState {
  export function fromBoolean(value: boolean) {
    return value ? CheckedState.Checked : CheckedState.Unchecked;
  }

  /// Update a checkbox element according to a given checked state.
  export function updateCheckbox(element: HTMLInputElement, checkedState: CheckedState) {
    element.checked = checkedState === CheckedState.Checked;
    element.indeterminate = checkedState === CheckedState.Indeterminate;
  }
}
