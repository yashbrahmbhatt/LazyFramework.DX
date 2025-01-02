<script lang="ts">
	import { createSvelteTable, renderComponent } from '$lib/components/ui/data-table';
	import { getCoreRowModel, type ColumnDef } from '@tanstack/table-core';
	import * as Table from '$lib/components/ui/table';
	import FlexRender from '$lib/components/ui/data-table/flex-render.svelte';
	import Editable from './editable.svelte';
	import Select from './select.svelte';
	import EditTable from '$lib/components/ui/design-editor/design-editor.svelte';
	interface Row {
		id: number;
		name: string;
		spouse: number;
	}
	let data = $state([
		{ id: 1, name: 'John Doe', spouse: 2 },
		{ id: 2, name: 'Jane Doe', spouse: 1 },
		{ id: 3, name: 'John Smith', spouse: 4 },
		{ id: 4, name: 'Jane Smith', spouse: 3 }
	]);
	let spouseOptions = $derived(data.map((v) => ({ label: v.name, value: v.id.toString() })));
	let columns: ColumnDef<Row>[] = [
		{
			header: 'ID',
			accessorKey: 'id',
			cell: (ctx) =>
				renderComponent(Editable, {
					value: ctx.row.original.id,
					updated: (v) => (ctx.row.original.id = v)
				})
		},
		{
			header: 'Name',
			accessorKey: 'name',
			cell: (ctx) =>
				renderComponent(Editable, {
					value: ctx.row.original.name,
					updated: (v) => (ctx.row.original.name = v)
				})
		},
		{
			header: 'Spouse',
			accessorKey: 'spouse',
			cell: (ctx) =>
				renderComponent(Select, {
					value: ctx.row.original.spouse.toString(),
					options: spouseOptions
				})
		}
	];
	let table = createSvelteTable({
		data,
		columns,
		getCoreRowModel: getCoreRowModel()
	});
</script>
