<script lang="ts" module>
	import { getCoreRowModel, type CellContext, type ColumnDef } from '@tanstack/table-core';
	import { createSvelteTable, renderComponent } from '../data-table';
	import * as Table from '../table';
	import FlexRender from '../data-table/flex-render.svelte';
	import * as Tooltip from '../tooltip';
	import { CircleHelp, Plus } from 'lucide-svelte';

	export type DataTableProps<TData, TValue> = {
		columns: ColumnDef<TData, TValue>[];
		data: TData[];
	};
</script>

<script lang="ts">
	import { type Writable } from 'svelte/store';
	import ScrollArea from '../scroll-area/scroll-area.svelte';
	import * as Tabs from '../tabs';
	import DataTable from './data-table.svelte';

	let {
		data: dataInput = $bindable(),
		columns: columnsInput = $bindable()
	}: { data: Writable<Record<string, any>>; columns: Writable<Record<string, ColumnDef<any>[]>> } =
		$props();
	let data = $state($dataInput);
	let columns = $state($columnsInput);
	let tableObjs = Object.keys(data).reduce(
		(acc, key) => {
			return {
				...acc,
				[key]: createSvelteTable({
					data: data[key],
					columns: columns[key],
					getCoreRowModel: getCoreRowModel()
				})
			};
		},
		{} as Record<string, any>
	);
	const tables = $derived(tableObjs);
</script>

{#snippet EditTable({
	table,
	key,
	onNew
}: {
	table: ReturnType<typeof createSvelteTable>;
	key: string;
	onNew: () => void;
})}
	<Tabs.Content class="max-h-full" value={key}>
		<ScrollArea
			orientation="both"
			scrollbarYClasses="w-1 bg-accent rounded-lg"
			class=" h-[90vh] p-2"
		>
			<DataTable {table} {onNew} />
		</ScrollArea>
	</Tabs.Content>
{/snippet}
{#each Object.keys(tables) as table}
	{@render EditTable({ table: tables[table], key: table, onNew: () => {} })}
{/each}
