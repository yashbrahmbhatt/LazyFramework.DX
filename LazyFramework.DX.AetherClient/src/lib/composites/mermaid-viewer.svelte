<script lang="ts">
	import type { RenderResult } from 'mermaid';
	import { writable, type Writable } from 'svelte/store';
	import mermaid from 'mermaid';
	import ScrollArea from '$lib/components/ui/scroll-area/scroll-area.svelte';
	import { onMount } from 'svelte';
	import Button from '$lib/components/ui/button/button.svelte';
	import {
		CircleHelp,
		ClipboardCheck,
		ClipboardX,
		Copy,
		CopyCheck,
		ExternalLink
	} from 'lucide-svelte';
	import { clipboard } from '$lib/actions/clipboard';
	import * as Tooltip from '$lib/components/ui/tooltip';

	let { value = $bindable() }: { value: string } = $props();
	let el: HTMLPreElement | undefined;
	mermaid.initialize({
		startOnLoad: true,
		darkMode: true,
		theme: 'dark',
		state: {
			defaultRenderer: 'elk',
			useMaxWidth: true
		},
		look: 'classic',
		flowchart: {
			curve: 'cardinal',
			useMaxWidth: true,

			padding: 8,
			defaultRenderer: 'elk',
			titleTopMargin: 20,
			subGraphTitleMargin: {
				top: 8,
				bottom: 8
			}
		},
		elk: {
			mergeEdges: true,
			nodePlacementStrategy: 'SIMPLE'
		},
		block: {
			padding: 8,
			useMaxWidth: true
		}
	});
	let icon = $state('ready');
	$effect(() => {
		icon !== 'ready' ? setTimeout(() => (icon = 'ready'), 1000) : null;
	});
</script>

<div class="flex flex-row gap-4">
	<div
		use:clipboard={value}
		class=""
		onfailed={() => (icon = 'failed')}
		onsuccess={() => (icon = 'success')}
	>
		<Button variant={icon === 'failed' ? 'destructive' : 'default'}>
			{#if icon === 'ready'}
				<Copy />
			{:else if icon === 'success'}
				<ClipboardCheck />
			{:else}
				<ClipboardX />
			{/if}
		</Button>
	</div>
	<Button
		class="flex flex-row gap-2 text-foreground"
		variant="ghost"
		href="https://app.diagrams.net/"
		target="_blank"
	>
		Open Drawio
		<ExternalLink class="stroke-foreground" />
	</Button>
	<Tooltip.Root>
		<Tooltip.Trigger>
			<Button
				class="flex flex-row gap-2 text-foreground"
				variant="ghost"
				href="https://www.drawio.com/blog/mermaid-diagrams"
				target="_blank"
			>
				Help
				<CircleHelp class="stroke-foreground" />
			</Button>
		</Tooltip.Trigger>
		<Tooltip.Content>Copy to clipboard</Tooltip.Content>
	</Tooltip.Root>
</div>
<ScrollArea orientation="both" class="min-h-full  min-w-full">
	<pre bind:this={el} class="min-h-full min-w-full">
		{#await mermaid.render('mermaid', value)}
			...Loading
		{:then diagram}
			{@html diagram.svg}
		{/await}
		</pre>
</ScrollArea>
