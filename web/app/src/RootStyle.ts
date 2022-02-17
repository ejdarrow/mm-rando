export const rootStyle = () => {
  return document.documentElement.style;
}

export const setItemPoolColumnCount = (count: number) => {
  rootStyle().setProperty('--app-itempool-column-count', count.toString());
}
