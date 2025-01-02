<script lang="ts">
	import * as Select from '$lib/components/ui/select';
	let {
		value = $bindable(),
		options
	}: {
		value: string;
		options: {
			label: string;
			value: string;
		}[];
	} = $props();
</script>

<Select.Root type="single" bind:value>
	<Select.Trigger>{options.find((v) => v.value === value)?.label}</Select.Trigger>
	<Select.Content>
		{#each options as option}
			<Select.Item value={option.value}>{option.label}</Select.Item>
		{/each}
	</Select.Content>
</Select.Root>
