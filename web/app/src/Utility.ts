export const isHexString = (str: string) => {
  for (var i = 0; i < str.length; i++) {
    const c = str[i];
    if ('0' <= c && c <= '9')
      continue;
    else if ('a' <= c && c <= 'f')
      continue;
    else if ('A' <= c && c <= 'F')
      continue;
    else
      return false;
  }
  return true;
};

export const tuple = <T extends any[]>(...args: T): T => args;

export namespace u32 {
  export const MAX = 0xFFFFFFFF;
}
