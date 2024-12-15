<script lang="ts">
	import type { Snippet } from 'svelte';

	let {
		isSelected,
		onClick,
		item
	}: {
		isSelected: boolean;
		onClick: () => void;
		item: {
			id: string;
			label: string;
			icon?: Snippet;
		};
	} = $props();
</script>

<!-- svelte-ignore a11y_click_events_have_key_events -->
<!-- svelte-ignore a11y_no_static_element_interactions -->
<div
	class="flex cursor-pointer items-center px-2 py-2 hover:bg-muted"
	class:selected={isSelected}
	onclick={onClick}
>
	{#if item.icon}
		{@render item.icon()}
	{/if}
	<span class="truncate text-sm">{item.label}</span>
</div>
