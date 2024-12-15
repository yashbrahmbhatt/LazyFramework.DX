<script lang="ts">
	import '$lib/composites/NavBar.svelte';
	import { onMount, setContext } from 'svelte';
	import '../app.css';
	import { ModeWatcher } from 'mode-watcher';
	import LogDrawer from '$lib/composites/LogDrawer.svelte';
	import NavBar from '$lib/composites/NavBar.svelte';
	import { writable } from 'svelte/store';
	
	let { children, data } = $props();

	let logOpen: boolean = $state(false);

	function toggleLog() {
		logOpen = !logOpen;
	}
	
</script>

<div class='font-mono'>
	<ModeWatcher />
	<NavBar toggleLog={toggleLog} />
	<LogDrawer bind:open={logOpen} />
	{@render children()}
</div>
