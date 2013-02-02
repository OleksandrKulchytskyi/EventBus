echo "Loading System.Messaging..."
[Reflection.Assembly]::LoadWithPartialName( "System.Messaging" )
$msmq = [System.Messaging.MessageQueue]
 
$queueList = ( "test", "payload")
 
echo ""
echo "(Re)creating the queues and setting permissions to Everyone/FullControl:"
foreach($qName in $queueList){
        $qName = ".\private$\" + $qName
        if($msmq::Exists($qName)){
            echo ("    " + $qName + " already exists and will be deleted and recreated")
            $msmq::Delete($qName)
        }
        $q = $msmq::Create( $qName )
        $q.UseJournalQueue = $TRUE
        $q.MaximumJournalSize = 1024 #kilobytes
        $q.SetPermissions("Everyone", [System.Messaging.MessageQueueAccessRights]::FullControl, [System.Messaging.AccessControlEntryType]::Set)
    }
echo "All queues processed."
echo ""
 
echo "Listing existing private queues:"
echo ""
foreach($q in $msmq::GetPrivateQueuesByMachine(".")){
    echo ("    " + $q.QueueName)
}
 
echo ""
echo "Done."