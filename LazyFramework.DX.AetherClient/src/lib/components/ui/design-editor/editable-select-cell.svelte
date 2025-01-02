<script lang="ts" module>
	export interface SelectOption {
		value: string;
		label: string;
	}
	export type EditableSelectCellProps = EditableBase & {
		getOptions: (storeValue: any, context: CellContext<any, unknown>) => SelectOption[];
	};
</script>

<script lang="ts">
	import Switch from '$lib/components/ui/switch/switch.svelte';
	import Label from '$lib/components/ui/label/label.svelte';
	import type { EditableBase } from './editable-base';
	import type { CellContext } from '@tanstack/table-core';
	import * as Select from '$lib/components/ui/select';

	let { getOptions, context, setValue, store, getValue, validations }: EditableSelectCellProps =
		$props();
	let value = $state(getValue($store, context));
	let options = $state(getOptions($store, context));
	let errors = $state(validate());

	store.subscribe(() => {
		value = getValue($store, context);
		options = getOptions($store, context);
		errors = validate();
	});
	function validate() {
		return validations.map((validation) => validation(value, $store, context));
	}

	function update() {
		if (errors.every((error) => error === null)) {
			setValue(store, value, context);
		}
	}
</script>

<div class="flex w-full flex-col gap-1.5">
	<div class="flex items-center space-x-2">
		<Select.Root type="single" bind:value>
			<Select.Trigger>{options.find((v) => v.value === value)?.label}</Select.Trigger>
			<Select.Content>
				{#each options as option}
					<Select.Item value={option.value}>{option.label}</Select.Item>
				{/each}
			</Select.Content>
		</Select.Root>
	</div>
	{#each errors as error}
		{#if error}
			<p class="text-wrap text-xs text-destructive">{error}</p>
		{/if}
	{/each}
</div>
