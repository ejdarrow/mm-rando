import { PropsWithChildren } from 'react'
import styles from './styles/Select.module.css'

interface SelectProps {
  id?: string
  label?: string
  onChange?: (event: React.ChangeEvent<HTMLSelectElement>) => void
}

const Select = (props: PropsWithChildren<SelectProps>) => {
  const renderLabel = () => {
    if (props.label !== undefined) {
      return (
        <label className="hidden mb-8 lg:mb-3 font-semibold" htmlFor={props.id}>
          {props.label}
        </label>
      )
    }
  }

  return (
    <>
      {renderLabel()}
      <div className={styles['select']}>
        <select className='leading-4' id={props.id} onChange={props.onChange}>
          {props.children}
        </select>
        <span className={styles['focus']}></span>
      </div>
    </>
  )
}

export default Select
