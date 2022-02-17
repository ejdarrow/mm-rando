import React from 'react';

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

interface NavProps {
  selected?: string;
  onNavItemClick: (event: React.MouseEvent<HTMLAnchorElement, MouseEvent>, identifier: string) => void;
}

class Nav extends React.Component<NavProps> {
  sections: NavSection[];

  constructor(props: NavProps) {
    super(props);
    this.sections = [
      new NavSection(new NavItem('Randomizer'), [
        new NavItem('Item Pool'),
        new NavItem('Extra Starting Items'),
        new NavItem('Junk Locations'),
      ]),
      new NavSection(new NavItem('Comfort')),
      new NavSection(new NavItem('Cosmetics'), [
        new NavItem('HUD Colors'),
      ]),
    ];
  }

  isSelected(identifier: string) {
    return this.props.selected !== undefined ? this.props.selected === identifier : false;
  }

  renderChildNavItem(item: NavItem) {
    if (this.isSelected(item.identifier)) {
      return (
        <li key={item.identifier}>
          <a
            className="font-semibold h-full py-1 block border-l-2 pl-4 -ml-2px border-transparent border-purple-500 text-purple-500"
            href="#"
            onClick={(event) => this.props.onNavItemClick(event, item.identifier)}
          >
            {item.name}
          </a>
        </li>
      )
    } else {
      return (
        <li key={item.identifier}>
          <a
            className="h-full py-1 block border-l-2 pl-4 -ml-2px border-transparent hover:border-neutral-300 hover:text-[#f8f8f8]"
            href="#"
            onClick={(event) => this.props.onNavItemClick(event, item.identifier)}
          >
            {item.name}
          </a>
        </li>
      )
    }
  }

  renderRootNavItem(item: NavItem) {
    if (this.isSelected(item.identifier)) {
      return (
        <a
          className="block mb-8 lg:mb-3 font-semibold decoration-violet-500 underline"
          href="#"
          onClick={(event) => this.props.onNavItemClick(event, item.identifier)}
        >
          {item.name}
        </a>
      )
    } else {
      return (
        <a
          className="block mb-8 lg:mb-3 font-semibold decoration-neutral-500 hover:underline"
          href="#"
          onClick={(event) => this.props.onNavItemClick(event, item.identifier)}
        >
          {item.name}
        </a>
      )
    }
  }

  renderSection(section: NavSection) {
    let list;
    if (section.children && section.children.length > 0) {
      list = section.children.map(child => (
        this.renderChildNavItem(child)
      ));
    }

    return (
      <li className="mt-12 lg:mt-8" key={section.root.identifier}>
        {this.renderRootNavItem(section.root)}
        <ul className="border-l-2 border-neutral-700 text-sm space-y-6 lg:space-y-1">
          {list}
        </ul>
      </li>
    )
  }

  renderGeneratorSelect() {
    return (
      <div>
        <label className="hidden mb-8 lg:mb-3 font-semibold" htmlFor="generator-select">
          Generator
        </label>
        <div className="select">
          <select className="leading-4" id="generator-select">
            <option value="1.14.0.6 Release">1.14.0.6 Release</option>
            <option value="1.15.0.11-beta">1.15.0.11-beta</option>
          </select>
          <span className="focus"></span>
        </div>
      </div>
    )
  }

  render() {
    return (
      <>
        {this.renderGeneratorSelect()}
        <div className="my-separator h-1 mt-6 mb-2"></div>
        <ul className="ml-5px">
          {this.sections.map(section => this.renderSection(section))}
        </ul>
      </>
    )
  }
}

export default Nav;
