# Deployment

Create a redis instance:
```
helm install stable/redis --name redis-powercraft --values ./redis.values.yml --namespace powercraft
```

Create config map
```
kubectl apply -f ./config.map.yml -n powercraft
kubectl apply -f ./influx.secret.yml -n powercraft
kubectl apply -f ./regcred.yml -n powercraft
```

@todo port to Helm Chart and add manual deploy step in gitlab ci.
@todo Add Make file to deploy to local minikube 


Make sure you set the image name correctly.
Then deploy!

`kubectl apply -f ./deployment.yml -n powercraft`
`kubectl create -f ./migrations.yml -n powercraft`
`kubectl apply -f ./jobs.yml -n powercraft`