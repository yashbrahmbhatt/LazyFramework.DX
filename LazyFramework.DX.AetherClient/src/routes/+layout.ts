import type { Input } from '$lib/models/Parser';
import type { Solution } from '$lib/models/Solution';
import type { LayoutLoad } from './$types';
export const prerender = true;
export const ssr = false;
export const load = (async () => {
    let res = await fetch("http://localhost:7999/solution")
    let data = await res.json();
    return data as Input;
}) satisfies LayoutLoad;