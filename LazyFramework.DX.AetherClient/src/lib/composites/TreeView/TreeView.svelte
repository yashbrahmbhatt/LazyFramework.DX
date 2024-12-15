<script lang="ts">
	import { onMount, type Snippet } from 'svelte';
	import { writable } from 'svelte/store';
	import * as Accordion from '$lib/components/ui/accordion';
	import * as ScrollArea from '$lib/components/ui/scroll-area';
	import { ChevronRight } from 'lucide-svelte';
	import TreeItem from './TreeItem.svelte';

	let {
		data = $bindable(),
		initialSelectedItemId,
		onSelectChange,
		expandAll = false,

	}: {
    data: {
      id: string;
      label: string;
      children: any[];
      icon?: Snippet
    }[],
    initialSelectedItemId: string,
    onSelectChange: (item: any) => void,
    expandAll: boolean,

  } = $props();

	let selectedItemId = $state(writable(initialSelectedItemId));

	let expandedItemIds: string[] = [];

	function walkTreeItems(items: any, targetId: string) {
		if (Array.isArray(items)) {
			for (let item of items) {
				expandedItemIds.push(item.id);
				if (walkTreeItems(item.children || [], targetId) && !expandAll) {
					return true;
				}
				if (!expandAll) expandedItemIds.pop();
			}
		} else if (!expandAll && items.id === targetId) {
			return true;
		} else if (items.children) {
			return walkTreeItems(items.children, targetId);
		}
	}

	if (initialSelectedItemId) {
		walkTreeItems(data, initialSelectedItemId);
	}

	function handleSelectChange(item: any) {
		selectedItemId.set(item?.id);
		if (onSelectChange) {
			onSelectChange(item);
		}
	}
</script>

<div class="overflow-hidden">
	<ScrollArea.ScrollArea style="width: 100%; height: 100%;">
		<div class="relative p-2">
			<TreeItem
				{data}
				bind:selectedItemId
				{handleSelectChange}
				{expandedItemIds}
			/>
		</div>
	</ScrollArea.ScrollArea>
</div>

<style>
	.hover\:bg-muted:hover {
		background-color: var(--color-muted);
	}
	.selected {
		background-color: var(--color-accent);
		color: var(--color-accent-foreground);
	}
</style>
