<script lang="ts" module>
	export interface LogEntry {
		Level: string;
		Message: string;
		Timestamp: string;
		Context: string;
	}
</script>

<script lang="ts">
	import * as Drawer from '$lib/components/ui/drawer';
	import * as ToggleGroup from '$lib/components/ui/toggle-group';
	import * as Table from '$lib/components/ui/table';
	import { onMount } from 'svelte';
	import Button from '$lib/components/ui/button/button.svelte';
	import { X, ListRestart, FilterX } from 'lucide-svelte';
	import Input from '$lib/components/ui/input/input.svelte';
	let {
		open = $bindable(),
		logs
	}: {
		open: boolean;
		logs: LogEntry[];
	} = $props();

	var contexts = $derived(new Set(logs.map((log) => log.Context).sort()));
	var levels = $derived(new Set(logs.map((log) => log.Level).sort()));
	var selectedLevels: string[] = $state([]);
	var selectedContexts: string[] = $state([]);
	var filteredLogs = $derived(
		logs.filter(
			(log) =>
				selectedContexts.includes(log.Context) &&
				(searchString === '' || log.Message.includes(searchString)) &&
				(selectedLevels.length === 0 || selectedLevels.includes(log.Level.toString()))
		)
	);

	function getLogLevel(index: number): string {
		switch (index) {
			case 0:
				return 'Debug';
			case 1:
				return 'Info';
			case 2:
				return 'Warning';
			case 3:
				return 'Error';
			default:
				return 'UNKNOWN';
		}
	}

	function getLogColor(index: number): string {
		switch (index) {
			case 0:
				return 'text-foreground/50';
			case 1:
				return 'text-foreground';
			case 2:
				return 'text-yellow-500';
			case 3:
				return 'text-red-500';
			default:
				return 'text-gray-500';
		}
	}

	function formatTimestamp(timestamp: string): string {
		var date = new Date(timestamp);
		return `[${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}:${date.getSeconds().toString().padStart(2, '0')}]`;
	}

	let searchString: string = $state('');

	function clearFilters() {
		selectedContexts = [];
		searchString = '';
	}
</script>

<Drawer.Root bind:open>
	<Drawer.Content class="h-full">
		<div class="flex w-full flex-row place-items-center space-x-2 px-2 py-2">
			<Input class="ml-4 flex w-96 self-center" placeholder="Search" bind:value={searchString} />
			<Button variant="ghost" size="sm" onclick={() => (selectedContexts = [])}><FilterX /></Button>
			<ToggleGroup.Root
				variant="outline"
				class="flex h-full place-content-center self-start pl-10 pr-4"
				type="multiple"
				bind:value={selectedContexts}
			>
				{#each Array.from(contexts) as context}
					<ToggleGroup.Item class="self-center" value={context}>{context}</ToggleGroup.Item>
				{/each}
			</ToggleGroup.Root>
			<ToggleGroup.Root
				variant="outline"
				class="flex h-full place-content-center self-start pl-10 pr-4"
				type="multiple"
				bind:value={selectedLevels}
			>
				{#each Array.from(levels) as level}
					<ToggleGroup.Item class="self-center" value={level}
						>{getLogLevel(parseInt(level))}</ToggleGroup.Item
					>
				{/each}
			</ToggleGroup.Root>
			<div class="flex flex-auto"></div>
			<div class="flex flex-row space-x-2 place-self-end py-2 pr-4">
				<Button variant="ghost" class="" onclick={clearFilters}><ListRestart /></Button>
				<Button variant="ghost" onclick={() => (open = !open)}><X /></Button>
			</div>
		</div>
		<Table.Root>
			<Table.Header>
				<Table.Row>
					<Table.Head class="w-min text-center">Timestamp</Table.Head>
					{#if selectedContexts.length > 1}
						<Table.Head class="w-min text-center">Level</Table.Head>
					{/if}
					<Table.Head class="w-min text-center">Context</Table.Head>
					<Table.Head>Message</Table.Head>
				</Table.Row>
			</Table.Header>
			<Table.Body>
				{#each filteredLogs as log}
					<Table.Row class={`${getLogColor(parseInt(log.Level))}`}>
						<Table.Cell class="w-min p-1 text-center">{formatTimestamp(log.Timestamp)}</Table.Cell>
						<Table.Cell class="w-min p-1 text-center">{getLogLevel(parseInt(log.Level))}</Table.Cell
						>
						{#if selectedContexts.length > 1}
							<Table.Cell class="w-min p-1 text-center">{log.Context}</Table.Cell>
						{/if}
						<Table.Cell>{log.Message}</Table.Cell>
					</Table.Row>
				{/each}
			</Table.Body>
		</Table.Root>
	</Drawer.Content>
</Drawer.Root>
