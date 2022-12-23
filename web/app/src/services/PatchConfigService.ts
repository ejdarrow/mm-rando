export async function getLocalVersions(): Promise<string[]> {

    try {
        const response = await fetch('/Config/fetchLocalReleaseDependencies');
        return await response.json();
    } catch(error) {
        return [];
    }

}
