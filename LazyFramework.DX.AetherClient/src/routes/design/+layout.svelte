<script lang="ts">
	import type { Snippet } from 'svelte';
	import type { LayoutData } from './$types';
	import * as Sidebar from '$lib/components/ui/sidebar';
	import Separator from '$lib/components/ui/separator/separator.svelte';
	import * as Tabs from '$lib/components/ui/tabs';

	let { data, children }: { data: LayoutData; children: Snippet } = $props();
</script>

{#snippet TabTrigger({ value, text }: { value: string; text: string })}
	<Tabs.Trigger {value} class="data-[state=active]:bg-muted data-[state=active]:text-foreground"
		>{text}</Tabs.Trigger
	>
{/snippet}

<Sidebar.Inset>
	<Tabs.Root value="robots">
		<header class="flex h-16 shrink-0 items-center gap-2 px-4">
			<Sidebar.Trigger class="-ml-1" />
			<Separator orientation="vertical" class="mr-2 h-4 bg-sidebar-foreground/30" />
			<Tabs.List class="w-full justify-start gap-4 bg-inherit">
				{@render TabTrigger({ value: 'robots', text: 'Robots' })}
				{@render TabTrigger({ value: 'libraries', text: 'Libraries' })}
				{@render TabTrigger({ value: 'queues', text: 'Queues' })}
				{@render TabTrigger({ value: 'tasks', text: 'Tasks' })}
				{@render TabTrigger({ value: 'applications', text: 'Applications' })}
				{@render TabTrigger({ value: 'triggers', text: 'Triggers' })}
				{@render TabTrigger({ value: 'view', text: 'View' })}
			</Tabs.List>
		</header>
		<Separator class="bg-sidebar-foreground/30" />
		<div class="h-full">
			{@render children()}
		</div>
	</Tabs.Root>
</Sidebar.Inset>
