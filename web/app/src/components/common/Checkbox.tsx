import React, { useEffect, useRef } from 'react';
import { mergeRefs } from '../../common/Utility';
import { CheckedState } from '../../common/CheckedState';

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
    <input
      type="checkbox"
      className={props.className}
      ref={mergeRefs(props.inputRef, checkRef)}
      onChange={props.onChange}
      onClick={props.onClick}
    />
  );
};
