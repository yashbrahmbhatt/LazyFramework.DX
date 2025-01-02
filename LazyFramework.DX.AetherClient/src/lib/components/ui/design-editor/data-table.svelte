<script lang="ts">
	import * as Table from '$lib/components/ui/table';
	import FlexRender from '../data-table/flex-render.svelte';
	import * as Tooltip from '../tooltip';
	import { createSvelteTable } from '../data-table';
	import { CircleHelp, Plus } from 'lucide-svelte';
	import Button from '../button/button.svelte';

	let { table, onNew }: { table: ReturnType<typeof createSvelteTable>; onNew?: () => void } =
		$props();
</script>

{#snippet EditTable({
	table,
	onNew
}: {
	table: ReturnType<typeof createSvelteTable>;
	onNew?: () => void;
})}
	<Table.Root>
		<Table.Header>
			{#if table}
				{#each table.getHeaderGroups() as headerGroup (headerGroup.id)}
					<Table.Row>
						{#each headerGroup.headers as header (header.id)}
							<Table.Head>
								{#if !header.isPlaceholder}
									<div
										class="flex h-full flex-row place-content-center place-items-center gap-2 text-center"
									>
										<FlexRender
											content={header.column.columnDef.header}
											context={header.getContext()}
										/>
										{#if header.column.columnDef.meta ? (header.column.columnDef.meta as any).tooltip !== undefined : false}
											<Tooltip.Provider>
												<Tooltip.Root>
													<Tooltip.Trigger>
														<CircleHelp class="size-4" />
													</Tooltip.Trigger>
													<Tooltip.Content>
														{(header.column.columnDef.meta as any).tooltip}
													</Tooltip.Content>
												</Tooltip.Root>
											</Tooltip.Provider>
										{/if}
										<div class="flex-auto"></div>
									</div>
								{/if}
							</Table.Head>
						{/each}
					</Table.Row>
				{/each}
			{/if}
		</Table.Header>
		<Table.Body>
			{#each table.getRowModel().rows as row (row.id)}
				<Table.Row data-state={row.getIsSelected() && 'selected'}>
					{#each row.getVisibleCells() as cell (cell.id)}
						<Table.Cell class="">
							<!-- <div
							class="flex h-full w-full flex-row place-content-center place-items-center text-center"
						> -->
							<FlexRender content={cell.column.columnDef.cell} context={cell.getContext()} />
							<!-- </div> -->
						</Table.Cell>
					{/each}
				</Table.Row>
			{:else}
				<Table.Row>
					<Table.Cell colspan={table.getAllColumns().length} class="h-24 text-center ">
						<span class="text-center text-sm text-sidebar-foreground/50"> No data </span>
					</Table.Cell>
				</Table.Row>
			{/each}
		</Table.Body>
	</Table.Root>
	{#if onNew}
		<Button class="w-full" variant="ghost" onclick={onNew}>
			<Plus />
		</Button>
	{/if}
{/snippet}

{@render EditTable({ table, onNew })}
