<script lang="ts">
	import * as Drawer from '$lib/components/ui/drawer';
	import * as ToggleGroup from '$lib/components/ui/toggle-group';
	import * as Table from '$lib/components/ui/table';
	import { onMount } from 'svelte';

	let logs: {
		Level: string;
		Message: string;
		Timestamp: string;
		Context: string;
	}[] = [];

	onMount(async () => {
		try {
			const ws = new WebSocket('ws://localhost:7999/');

			ws.addEventListener('open', () => {
				console.log('WebSocket connection established.');
			});

			ws.addEventListener('message', (event) => {
				const message = event.data;
				try {
					const newLogs = JSON.parse(message);
					logs = newLogs;
					console.log(logs);
				} catch (error) {
					console.error('Error parsing message:', error);
				}
			});

			ws.addEventListener('close', () => {
				console.log('WebSocket connection closed.');
			});

			ws.addEventListener('error', (error) => {
				console.error('WebSocket error:', error);
			});
		} catch (error) {
			console.error('Error connecting to WebSocket:', error);
		}
	});
	var contexts = $derived(new Set(logs.map((log) => log.Context).sort()));
	var selectedContexts: string[] = $state([]);
	var filteredLogs = $derived(logs.filter((log) => selectedContexts.includes(log.Context)));

	function getLogLevel(index: number): string {
		switch (index) {
			case 0:
				return 'DEBUG';
			case 1:
				return 'INFO';
			case 2:
				return 'WARNING';
			case 3:
				return 'ERROR';
			default:
				return 'UNKNOWN';
		}
	}

	function getLogColor(index: number): string {
		switch (index) {
			case 0:
				return 'bg-blue-500';
			case 1:
				return 'bg-green-500';
			case 2:
				return 'bg-yellow-500';
			case 3:
				return 'bg-red-500';
			default:
				return 'bg-gray-500';
		}
	}

    function formatTimestamp(timestamp: string): string {
        var date = new Date(timestamp);
        return `[${date.getHours()}:${date.getMinutes()}:${date.getSeconds()}]`;
    }
</script>
<div>

    <Drawer.Root>
        <Drawer.Trigger>Logs</Drawer.Trigger>
        <Drawer.Content class='h-1/2'>
            <div class='flex flex-row py-2'>

                <ToggleGroup.Root class='self-start' type="multiple" bind:value={selectedContexts}>
                    {#each Array.from(contexts) as context}
                    <ToggleGroup.Item value={context}>{context}</ToggleGroup.Item>
                    {/each}
                </ToggleGroup.Root>
            </div>
            <Table.Root>
                <Table.Header>
                    <Table.Row>
                        <Table.Head class="w-min">Level</Table.Head>
                        {#if selectedContexts.length > 1}
                        <Table.Head>Context</Table.Head>
                        {/if}
                        <Table.Head>Message</Table.Head>
                        <Table.Head>Timestamp</Table.Head>
                    </Table.Row>
                </Table.Header>
                <Table.Body>
                    {#each filteredLogs as log}
					<Table.Row class={`${getLogColor(parseInt(log.Level))}`}>
						<Table.Cell>{formatTimestamp(log.Timestamp)}</Table.Cell>
                        <Table.Cell>{getLogLevel(parseInt(log.Level))}</Table.Cell>
                        {#if selectedContexts.length > 1}
						<Table.Cell>{log.Context}</Table.Cell>
                        {/if}
						<Table.Cell>{log.Message}</Table.Cell>
					</Table.Row>
                    {/each}
                </Table.Body>
            </Table.Root>
        </Drawer.Content>
    </Drawer.Root>
</div>

<style>

</style>
