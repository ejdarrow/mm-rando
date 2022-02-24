import React, { useEffect, useRef } from 'react';
import { mergeRefs } from './Utility';

export enum CheckedState {
  Unchecked,
  Checked,
  Indeterminate,
}

export namespace CheckedState {
  export function fromBoolean(value: boolean) {
    return value ? CheckedState.Checked : CheckedState.Unchecked;
  };

  /// Update a checkbox element according to a given checked state.
  export function updateCheckbox(element: HTMLInputElement, checkedState: CheckedState) {
    element.checked = checkedState === CheckedState.Checked;
    element.indeterminate = checkedState === CheckedState.Indeterminate;
  }
}

export interface CheckboxProps {
  className?: string;
  onClick?: (event: React.MouseEvent<HTMLInputElement>) => void;
  onChange?: (event: React.ChangeEvent<HTMLInputElement>) => void;
  inputRef?: React.RefObject<HTMLInputElement>;
  value: CheckedState;
}

export const Checkbox = (props: CheckboxProps) => {
    const checkRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
      if (checkRef.current !== null) {
        CheckedState.updateCheckbox(checkRef.current, props.value);
      }
    });

    return (
      <input type="checkbox" className={props.className} ref={mergeRefs(props.inputRef, checkRef)} onChange={props.onChange} onClick={props.onClick} />
    )
}
