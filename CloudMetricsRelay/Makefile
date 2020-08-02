

up:
	docker-compose down
	docker-compose up --build -d
	print "\n\n\n\n\n\n\n\n"
	print "Metrics Relay should be up by now ;) \n"
	print "Push metrics via websocket chrome plugin to ws://localhost:8080/metrics or curl :P"
	print "test data is ./resources/sample_br_metric.json"
	print "http://localhost:8080/BR_Metrics/show-metrics"

down:
	docker-compose down

tail:
	docker-compose logs -f

send-tick:
	cat ./ressources/sample_br_metric.json | websocat ws://localhost:8080/metrics

show-br:
	xdg-open "http://localhost:8080/BR_Metrics/show-metrics"