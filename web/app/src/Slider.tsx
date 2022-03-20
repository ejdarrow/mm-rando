import React from 'react';
import './Slider.css';

interface SliderProps {
  defaultChecked?: boolean;
  onChange?: (event: React.ChangeEvent<HTMLInputElement>) => void;
}

export const Slider = (props: SliderProps) => {
  return (
    <label className="switch">
      <input type="checkbox" defaultChecked={props.defaultChecked} onChange={props.onChange} />
      <span className="slider rounded"></span>
    </label>
  );
};

export default Slider;
