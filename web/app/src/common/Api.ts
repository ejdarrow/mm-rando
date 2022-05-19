// Reference: https://stackoverflow.com/a/49471725
export function api<T>(url: string): Promise<T> {
  return fetch(url)
    .then((response) => {
      if (!response.ok) {
        throw new Error(response.statusText);
      }
      return response.json() as Promise<T>;
    })
    .catch((error: Error) => {
      console.error(error);
      throw error;
    });
}
