import React from 'react';
import styles from './Slider.module.css';

interface SliderProps {
  defaultChecked?: boolean;
  onChange?: (event: React.ChangeEvent<HTMLInputElement>) => void;
}

export const Slider = (props: SliderProps) => {
  return (
    <label className={styles['switch']}>
      <input type="checkbox" defaultChecked={props.defaultChecked} onChange={props.onChange} />
      <span className={`${styles['slider']} rounded`}></span>
    </label>
  );
};

export default Slider;
