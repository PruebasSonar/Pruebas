


// vars
var mapTextContainer;
var map;

// ==================================================
//
//   Capture/Display Map
//   function to be invoked from form.
//
function captureMap(imageContainer, textContainer, isEditable, defaultLatitude, defaultLongitude) {
    // prepare map environment
    if (imageContainer?.length > 0) {
        mapTextContainer = `#${textContainer}`;
        displayMap(imageContainer, $(mapTextContainer).text(), isEditable, defaultLatitude, defaultLongitude);
    }
}

//   Display Map
function displayMap(imageContainer, featureCollection, isEditable, defaultLatitude, defaultLongitude) {
    // prepare map environment
    if (imageContainer?.length > 0) {
        var centro = [defaultLatitude, defaultLongitude];
        var zoom = 13;
        // mapa
        map = L.map(imageContainer).setView(centro, zoom);
        map.pm.setLang("es");

        switchBasemap();

        if (isEditable) {

            // geocoder:
            L.Control.geocoder({
                defaultMarkGeocode: false,
                collapsed: true, // para que aparezca abierto o cerrado
                placeholder: 'Buscar..'
            })
                .on('markgeocode', (e) => {
                    var bbox = e.geocode.bbox;
                    var poly = L.polygon([
                        bbox.getSouthEast(),
                        bbox.getNorthEast(),
                        bbox.getNorthWest(),
                        bbox.getSouthWest()
                    ]);
                    map.fitBounds(poly.getBounds());
                })
                .addTo(map);
        }

        setControls(isEditable);

        presentMap(featureCollection);
    }
}

// Desplegar Geojson en el mapa 
function presentMap(featureCollectionString) {
    // load
    // <-- convertir el geojson desde la base, y desplegarlo en el mapa
    if (featureCollectionString?.length) {
        //'{"type":"FeatureCollection","features":[{"type":"Feature","properties":{ },"geometry":{"type":"Point","coordinates":[-58.381691,-34.550478]}},{"type":"Feature","properties":{ },"geometry":{"type":"Point","coordinates":[-58.326588,-34.545388]}},{"type":"Feature","properties":{ },"geometry":{"type":"Point","coordinates":[-58.328304,-34.567866]}}]}';
        var featureCollection = JSON.parse(featureCollectionString);
        var geometryLayerGroup = L.layerGroup();  // LayerGroup para almacenar las geometrías

        // Agregar geometrías iniciales al LayerGroup
        featureCollection.features.forEach(function (feature) {
            // itera por cada feature del feature collection,
            // crea una capa tipo geojson, y la agrega al mapa
            var layer = L.geoJSON(feature).addTo(map);
            geometryLayerGroup.addLayer(layer);
        });

        // Calculate bounds of all features in the LayerGroup
        var bounds = getLayerGroupBounds(geometryLayerGroup);
        if (bounds.isValid()) {
            map.flyToBounds(bounds);
        }

    }
}


// obtener el FeatureCollection
function getMapInfo() {
    if (mapTextContainer != null) {
        featureCollection = map.pm.getGeomanLayers(true).toGeoJSON();
        var featureCollectionStringify = JSON.stringify(featureCollection);
        //actualiza en el input el resultado
        $(mapTextContainer).text(featureCollectionStringify);
    }
}




// Helper function to get bounds of a LayerGroup
function getLayerGroupBounds(layerGroup) {
    var groupBounds = null;

    layerGroup.eachLayer(function (layer) {
        if (groupBounds === null) {
            groupBounds = layer.getBounds();
        } else {
            groupBounds.extend(layer.getBounds());
        }
    });

    return groupBounds;
}


// sets available controls depending on mode (isEditable: true=edit false=display)
function setControls(isEditable) {

    // Agregar el plugin Leaflet-Geoman para edición
    if (isEditable) {
        // set parameters for edit
        map.pm.addControls({
            drawText: false,
            drawCircleMarker: false,
            drawCircle: false,
            drawRectangle: false,
            drawPolyline: true,
            drawMarker: true,
            drawPolygon: true,
            editMode: true,
            dragMode: false,
            scrollmode: false,
            cutPolygon: false,
            removalMode: true,
            rotateMode: false
        });
    }
    else {
        // set parameters for display
        map.pm.addControls({
            drawText: false,
            drawCircleMarker: false,
            drawCircle: false,
            drawRectangle: false,
            drawPolyline: false,
            drawMarker: false,
            drawPolygon: false,
            editMode: false,
            dragMode: false,
            cutPolygon: false,
            removalMode: false,
            rotateMode: false
        });

    }
}

// Switch Base Map
function switchBasemap() {

    // new basemap switch:
    new L.basemapsSwitcher([
        {
            layer: L.tileLayer('http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            }).addTo(map), //DEFAULT MAP
            icon: '/assets/img/map/osm_logo.png',
            name: 'OSM'
        },
        {
            layer: L.tileLayer('https://wms.ign.gob.ar/geoserver/gwc/service/tms/1.0.0/capabaseargenmap@EPSG%3A3857@png/{z}/{x}/{-y}.png', {
                attribution: '<a href="https://www.ign.gob.ar/AreaServicios/Argenmap/Introduccion" target="_blank">Instituto Geográfico Nacional</a> + <a href="https://www.osm.org/copyright" target="_blank">OpenStreetMap</a>',
                minZoom: 1,
                maxZoom: 20
            }),
            icon: '/assets/img/map/argenmap_logo.jpg',
            name: 'Argenmap'
        }
    ], { position: 'topright' }).addTo(map);

}


