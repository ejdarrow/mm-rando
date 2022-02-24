export type CompareFn<T> = (a: T, b: T) => number;

export function coalesceNull<TInput, TOutput>(x: TInput | null, fn: (arg: TInput) => TOutput) {
  return x !== null ? fn(x) : null;
}

export function coalesceUndef<TInput, TOutput>(x: TInput | undefined, fn: (arg: TInput) => TOutput) {
  return x !== undefined ? fn(x) : undefined;
}

export function filterDefined<T>(array: Array<T | undefined>) {
  return array.filter(x => x !== undefined) as unknown as Array<T>;
}

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

// Reference: https://www.davedrinks.coffee/how-do-i-use-two-react-refs/
export function mergeRefs<T>(...refs: Array<React.RefObject<T> | undefined>) {
  const filteredRefs = filterDefined(refs);
  if (!filteredRefs.length) return null;
  if (filteredRefs.length === 1) return filteredRefs[0];
  return (inst: T) => {
    for (const ref of filteredRefs) {
      const mutable = ref as React.MutableRefObject<T>;
      mutable.current = inst;
    }
  };
};

export const tuple = <T extends any[]>(...args: T): T => args;

export namespace u32 {
  export const MAX = 0xFFFFFFFF;
}
