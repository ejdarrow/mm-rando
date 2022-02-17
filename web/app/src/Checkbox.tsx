import React, { useEffect, useRef } from 'react';

export enum CheckedState {
  Unchecked,
  Checked,
  Indeterminate,
}

export namespace CheckedState {
  export function fromBoolean(value: boolean) {
    return value ? CheckedState.Checked : CheckedState.Unchecked;
  };
}

export interface CheckboxProps {
  onClick?: (event: React.MouseEvent<HTMLInputElement>) => void;
  value: CheckedState;
}

export const Checkbox = (props: CheckboxProps) => {
    const checkRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
      if (checkRef.current !== null) {
        checkRef.current.checked = props.value === CheckedState.Checked;
        checkRef.current.indeterminate = props.value === CheckedState.Indeterminate;
      }
    });

    return (
      <input type="checkbox" ref={checkRef} onClick={props.onClick} />
    )
}
