<script lang="ts" module>
	export interface TreeNode {
		id: string;
		type: string;
		label: string;
		value: any;
		children?: TreeNode[];
		expanded?: boolean;
	}
	export interface TreeNodeIcons {
		[key: string]: Snippet;
	}
</script>

<script lang="ts">
	let {
		tree = $bindable(),
		level = 0,
		icons,
		selected = $bindable()
	}: {
		tree: Array<SolutionTreeNode>;
		level: number;
		icons: Record<string, Snippet>;
		selected: Writable<TreeNode>;
	} = $props();
	if (level === 0) {
		selected = writable<SolutionTreeNode>(tree[0]);
	}

	function onSelect(value: any) {
		selected.set(value);
	}
	import * as Collapsible from '$lib/components/ui/collapsible';
	import Separator from '$lib/components/ui/separator/separator.svelte';
	import type { Snippet } from 'svelte';
	import Self from './FileTree.svelte';
	import { writable, type Writable } from 'svelte/store';
	import type { SolutionTreeNode } from '../../routes/solution/Solution';
</script>

<ul class="flex w-full flex-col {level === 0 ? `` : 'pl-3'}">
	{#each tree as node, i}
		<li class="relative flex w-full flex-row ">
			{#if node.children}
				{#if level !== 0}
					<Separator class="absolute place-self-center bg-foreground/50 {level > 1 ? "ml-3" : ""}" orientation="vertical" />
				{/if}
				<Collapsible.Root
					open={node.expanded}
					onOpenChange={() => (node.expanded = !node.expanded)}
					class="w-full"
				>
					<Collapsible.Trigger
						class="flex w-full place-content-start"
						onclick={() => onSelect(node)}
					>
						<span
							class="flex w-full flex-row place-items-center py-1
                        {$selected.id === node.id
								? 'mb-[-1px] border-b-[1px] border-b-foreground'
								: 'border-primary hover:mb-[-1px] hover:border-b-[1px]'}"
						>
							<Separator
								class="{level === 0 ? 'w-0' : 'w-4'} {level > 1 ? "ml-3" : ""} bg-foreground/50"
								orientation="horizontal"
							/>
							<div class="flex flex-row place-items-center">
								{#if Object.keys(icons).includes(node.type) || node.type === 'folder'}
									{#if node.type === 'folder'}
										{#if node.expanded}
											{@render icons.folderOpen()}
										{:else}
											{@render icons.folderClosed?.()}
										{/if}
									{:else}
										{@render icons[node.type]?.()}
									{/if}
								{/if}
								<div class="text-nowrap pl-2">
									{node.label}
								</div>
							</div>
						</span>
					</Collapsible.Trigger>
					<Collapsible.Content>
						{#if node.children}
							<Self tree={node.children} level={level + 1} {icons} {selected} />
							<!-- Recursive call to render subfolders/files -->
						{/if}
					</Collapsible.Content>
				</Collapsible.Root>
			{:else}
				{#if level !== 0}
					<Separator class="absolute h-4 bg-foreground/50 pb-2 {level > 0 ? "pl-3" : ""}" orientation="vertical" />
				{/if}
				<span
					role="treeitem"
					aria-selected="false"
					tabindex={i}
					class="flex cursor-pointer flex-row place-items-center py-1
                "
					onclick={() => onSelect(node)}
					onkeypress={(e) => e.key === 'Enter' && onSelect(node)}
				>
					{#if level !== 0}
						<Separator class="w-4 bg-foreground/50 " orientation="horizontal" />
					{/if}
					<div
						class="flex w-full flex-row place-items-center text-ellipsis text-nowrap
                    {$selected.id === node.id
							? 'mb-[-1px] border-b-[1px] border-b-foreground'
							: 'border-primary hover:mb-[-1px] hover:border-b-[1px]'}"
					>
						{@render icons.file()}
						<div class="pl-2">
							{node.label}
						</div>
					</div>
				</span>
			{/if}
		</li>
	{/each}
</ul>
