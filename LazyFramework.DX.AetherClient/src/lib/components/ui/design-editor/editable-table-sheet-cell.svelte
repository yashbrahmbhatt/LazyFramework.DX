<script lang="ts" module>
	export type EditableTableSheeetCellProps<T> = EditableBase & {
		getColumns: (
			originalStore: Writable<any>,
			newStore: Writable<any>,
			originalValue: any,
			newValue: any,
			context: CellContext<any, unknown>
		) => ColumnDef<any>[];
		getTriggerText: (value: any, context: CellContext<any, unknown>) => string;
	};
</script>

<script lang="ts">
	import { buttonVariants } from '$lib/components/ui/button';
	import * as Sheet from '$lib/components/ui/sheet';

	import { getCoreRowModel, type CellContext, type ColumnDef } from '@tanstack/table-core';

	import type { EditableBase } from './editable-base';
	import { writable, type Writable } from 'svelte/store';
	import DataTable from './data-table.svelte';
	import { createSvelteTable } from '../data-table';

	let {
		context,
		store,
		getValue,
		validations,
		setValue,
		getColumns,
		getTriggerText
	}: EditableTableSheeetCellProps<any> = $props();

	let skipStoreUpdate = false;
	let skipValueUpdate = false;

	// Create the nested value store
	let value = writable(getValue($store, context));

	// Update the value store when the root store changes
	store.subscribe((newStoreValue) => {
		if (skipValueUpdate) {
			skipValueUpdate = false;
			return;
		}
		skipStoreUpdate = true;
		value.set(getValue(newStoreValue, context));
	});

	// Update the root store when the value store changes
	value.subscribe((newValue) => {
		if (skipStoreUpdate) {
			skipStoreUpdate = false;
			return;
		}
		skipValueUpdate = true;
		// if (errors.every((error) => error === null)) {
		setValue(store, newValue, context);
		// }
	});

	// Handle columns and validations
	let columns = $derived(getColumns(store, value, $store, $value, context));
	let triggerText = $derived(getTriggerText($value, context));
	let errors = $state(validations.map((validation) => validation(value, $store, context)));

	let table = createSvelteTable({ data: $value, columns, getCoreRowModel: getCoreRowModel() });
</script>

<Sheet.Root>
	<Sheet.Trigger class="max-w-fit {buttonVariants({ variant: 'outline' })}">
		{triggerText}
	</Sheet.Trigger>
	<Sheet.Content class="min-w-fit">
		<Sheet.Header>
			<Sheet.Title>Manage Robot Queues</Sheet.Title>
			<Sheet.Description>
				Configure how the robot uses each of the queues in the solution
			</Sheet.Description>
		</Sheet.Header>
		<div>
			<DataTable {table} />
		</div>
	</Sheet.Content>
</Sheet.Root>
