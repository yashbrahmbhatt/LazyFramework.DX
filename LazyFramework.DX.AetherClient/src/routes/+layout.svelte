<script lang="ts">
	import '$lib/composites/NavBar.svelte';
	import '../app.css';
	import { ModeWatcher } from 'mode-watcher';
	import LogDrawer, { type LogEntry } from '$lib/composites/LogDrawer.svelte';
	import NavBar, { type SideBarLink } from '$lib/composites/NavBar.svelte';
	import * as Sidebar from '$lib/components/ui/sidebar';
	import * as Breadcrumb from '$lib/components/ui/breadcrumb';
	import Separator from '$lib/components/ui/separator/separator.svelte';
	import { FlaskConical, Hammer, Puzzle, WandSparkles, Wrench } from 'lucide-svelte';
	import { onMount } from 'svelte';
	import { writable } from 'svelte/store';
	import mermaid from 'mermaid';

	let { children, data } = $props();
	let ws = writable<WebSocket>(new WebSocket('ws://localhost:7999/'));
	let logs: LogEntry[] = $state([]);
	let logOpen: boolean = $state(false);
	onMount(async () => {
		try {
			$ws.addEventListener('open', () => {
				console.log('WebSocket connection established.');
			});

			$ws.addEventListener('message', (event) => {
				const message = event.data;
				try {
					const newLogs = JSON.parse(message);
					logs = newLogs;
				} catch (error) {
					console.error('Error parsing message:', error);
				}
			});

			$ws.addEventListener('close', () => {
				console.log('WebSocket connection closed.');
			});

			$ws.addEventListener('error', (error) => {
				console.error('WebSocket error:', error);
			});
		} catch (error) {
			console.error('Error connecting to WebSocket:', error);
		}
	});
	function toggleLogs() {
		logOpen = !logOpen;
	}

	const sidebarLinks: SideBarLink[] = [
		{
			title: 'Design',
			url: '/design',
			icon: PuzzleSnippet
		},
		{
			title: 'Solution',
			url: '/solution',
			icon: FlaskConicalSnippet
		},
		{
			title: 'Configurations',
			url: '/configurations',
			icon: WrenchSnippet
		},
		{
			title: 'Build',
			url: '/build',
			icon: HammerSnippet
		}
	];
	
	let sidebarOpen = $state(false);
</script>

<Sidebar.Provider bind:open={sidebarOpen} class="font-mono">
	<NavBar data={sidebarLinks} open={sidebarOpen} {toggleLogs} />
	<LogDrawer bind:open={logOpen} {logs} />
	{@render children()}
	<ModeWatcher />
</Sidebar.Provider>

{#snippet WandSparklesSnippet(props: any)}
	<WandSparkles {...props} />
{/snippet}
{#snippet FlaskConicalSnippet(props: any)}
	<FlaskConical {...props} />
{/snippet}
{#snippet WrenchSnippet(props: any)}
	<Wrench {...props} />
{/snippet}
{#snippet HammerSnippet(props: any)}
	<Hammer {...props} />
{/snippet}
{#snippet PuzzleSnippet(props: any)}
	<Puzzle {...props} />
{/snippet}
