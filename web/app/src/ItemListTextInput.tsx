import { useAppSelector } from './hooks'
import type { RootState } from './store'

interface ItemListTextInputProps {
  selector: (state: RootState) => string
}

const ItemListTextInput = (props: ItemListTextInputProps) => {
  const itemListString = useAppSelector(props.selector)
  return (
    <input
      className="font-mono w-full"
      placeholder="Item Pool String"
      type="text"
      readOnly
      value={itemListString}
    />
  )
}

export default ItemListTextInput
