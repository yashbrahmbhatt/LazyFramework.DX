<script lang='ts'>
	import { onMount } from "svelte";
    var response: any;
    onMount(async () => {
        try {

            var res = await fetch('http://localhost:7999/solution');
            response = await res.json();
        } catch(error) {
            response = "Error:" + error;
        }
    })
</script>
<pre>

    {JSON.stringify(response, null, "\t")}
</pre>
 