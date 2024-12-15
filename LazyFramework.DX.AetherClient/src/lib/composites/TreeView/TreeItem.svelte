<script lang="ts">
	import * as Accordion from '$lib/components/ui/accordion';
	import type { Snippet } from 'svelte';
	import Leaf from './Leaf.svelte';
	import TreeItem from './TreeItem.svelte';
	import type { Writable } from 'svelte/store';

	let { data, selectedItemId = $bindable(), handleSelectChange, expandedItemIds }: {
        data: {
            id: string;
            label: string;
            children: any[];
            icon?: Snippet;
        }[],
        selectedItemId: Writable<string>,
        handleSelectChange: (item: any) => void,
        expandedItemIds: string[]
    } = $props();
</script>

<ul role="tree">
	{#if Array.isArray(data)}
		{#each data as item (item.id)}
			<li>
				{#if item.children}
					<Accordion.Root multiple value={expandedItemIds}>
						<Accordion.Item value={item.id}>
							<Accordion.Trigger
								class="px-2 hover:bg-muted"
								on:click={() => handleSelectChange(item)}
							>
								{#if item.icon}
									{@render item.icon()}
								{/if}
								<span class="truncate text-sm">{item.label}</span>
							</Accordion.Trigger>
							<Accordion.Content class="pl-6">
								<TreeItem
									data={item.children}
									{selectedItemId}
									{handleSelectChange}
									{expandedItemIds}
								/>
							</Accordion.Content>
						</Accordion.Item>
					</Accordion.Root>
				{:else}
					<Leaf
						{item}
						isSelected={selectedItemId === item.id}
						onClick={() => handleSelectChange(item)}
					/>
				{/if}
			</li>
		{/each}
	{:else}
		<li>
			<Leaf
				item={data}
				isSelected={selectedItemId === data.id}
				onClick={() => handleSelectChange(data)}
			/>
		</li>
	{/if}
</ul>
