import React from 'react'
import Select from '../common/Select'

class NavItem {
  identifier: string;
  name: string;

  constructor(name: string, identifier?: string) {
    this.name = name;
    this.identifier = identifier ?? name.toLowerCase();
  }
}

class NavSection {
  root: NavItem;
  children?: NavItem[];

  constructor(root: NavItem, children?: NavItem[]) {
    this.root = root;
    this.children = children;
  }
}

const sections = [
  new NavSection(new NavItem('Main Settings'), [
  new NavItem('Generation Settings'),
  ]),
  new NavSection(new NavItem('Randomizer'), [
    new NavItem('Item Pool'),
    new NavItem('Extra Starting Items'),
    new NavItem('Junk Locations'),
  ]),
  new NavSection(new NavItem('Comfort')),
  new NavSection(new NavItem('Cosmetics'), [new NavItem('HUD Colors')]),
]

interface NavProps {
  selected?: string
  onNavItemClick: (event: React.MouseEvent<HTMLButtonElement, MouseEvent>, identifier: string) => void
}

const Nav = (props: NavProps) => {
  const isSelected = (identifier: string) => {
    return props.selected !== undefined ? props.selected === identifier : false
  }

  const renderChildNavItem = (item: NavItem) => {
    if (isSelected(item.identifier)) {
      return (
        <li key={item.identifier}>
          <button
            className="font-semibold h-full py-1 block border-l-2 pl-4 -ml-2px border-transparent border-purple-500 text-purple-500"
            onClick={(event) => props.onNavItemClick(event, item.identifier)}
          >
            {item.name}
          </button>
        </li>
      )
    } else {
      return (
        <li key={item.identifier}>
          <button
            className="h-full py-1 block border-l-2 pl-4 -ml-2px border-transparent hover:border-neutral-300 hover:text-[#f8f8f8]"
            onClick={(event) => props.onNavItemClick(event, item.identifier)}
          >
            {item.name}
          </button>
        </li>
      )
    }
  }

  const renderRootNavItem = (item: NavItem) => {
    if (isSelected(item.identifier)) {
      return (
        <button
          className="block mb-8 lg:mb-3 font-semibold decoration-violet-500 underline"
          onClick={(event) => props.onNavItemClick(event, item.identifier)}
        >
          {item.name}
        </button>
      )
    } else {
      return (
        <button
          className="block mb-8 lg:mb-3 font-semibold decoration-neutral-500 hover:underline"
          onClick={(event) => props.onNavItemClick(event, item.identifier)}
        >
          {item.name}
        </button>
      )
    }
  }

  const renderSection = (section: NavSection) => {
    let list
    if (section.children && section.children.length > 0) {
      list = section.children.map((child) => renderChildNavItem(child))
    }

    return (
      <li className="mt-12 lg:mt-8" key={section.root.identifier}>
        {renderRootNavItem(section.root)}
        <ul className="border-l-2 border-neutral-700 text-sm space-y-6 lg:space-y-1">{list}</ul>
      </li>
    )
  }

  const renderGeneratorSelect = () => {
    return (
      <div>
        <Select id="generator-select" label="Generator">
          <option value="v1.15.0.21">v1.15.0.21</option>
        </Select>
      </div>
    )
  }

  return (
    <>
      {renderGeneratorSelect()}
      <div className="my-separator h-1 mt-6 mb-2"></div>
      <ul className="ml-5px">{sections.map((section) => renderSection(section))}</ul>
    </>
  )
}

export default Nav
